using System.Security.Claims;
using System.Text;
using BeachTennis.Application.DTOs;
using BeachTennis.Application.Exceptions;
using BeachTennis.Application.UseCases;
using BeachTennis.Api.Security;
using BeachTennis.Domain.Interfaces;
using BeachTennis.Infrastructure.Persistence;
using BeachTennis.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings:DefaultConnection usando a variável de ambiente ConnectionStrings__DefaultConnection.");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
var issuer = builder.Configuration["Auth:Issuer"] ?? "BeachTennis.Api";
var audience = builder.Configuration["Auth:Audience"] ?? "BeachTennis.Front";
var signingKey = builder.Configuration["Auth:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Auth:SigningKey com pelo menos 32 bytes usando User Secrets ou um segredo no host.");
}

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Athlete", policy => policy.RequireRole("Athlete"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<AdminProvisioner>();
builder.Services.AddHostedService<AdminProvisioningService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeção de dependência (Repositórios)
builder.Services.AddScoped<IQuadraRepository, QuadraRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

// Casos de uso
builder.Services.AddScoped<AgendarReservaUseCase>();
builder.Services.AddScoped<CancelarReservaUseCase>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAtletaRepository, AtletaRepository>();

builder.Services.AddScoped<CriarQuadraUseCase>();
builder.Services.AddScoped<ListarQuadrasUseCase>();
builder.Services.AddScoped<ObterQuadraUseCase>();
builder.Services.AddScoped<AtualizarQuadraUseCase>();
builder.Services.AddScoped<DesativarQuadraUseCase>();

builder.Services.AddScoped<CriarAtletaUseCase>();
builder.Services.AddScoped<ListarAtletasUseCase>();
builder.Services.AddScoped<ObterAtletaUseCase>();
builder.Services.AddScoped<AtualizarAtletaUseCase>();
builder.Services.AddScoped<AutenticarAtletaUseCase>();

var app = builder.Build();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Endpoints de Quadras ---
app.MapGet("/api/quadras", async (ListarQuadrasUseCase useCase) =>
    Results.Ok(await useCase.ExecutarAsync()));

app.MapGet("/api/admin/quadras", async (IQuadraRepository repository) =>
    Results.Ok((await repository.ListarTodasAsync())
        .Select(quadra => new QuadraResponseDto(quadra.Id, quadra.Nome, quadra.Ativa))))
    .RequireAuthorization("Admin");

app.MapGet("/api/quadras/{id:guid}", async (Guid id, ObterQuadraUseCase useCase) =>
{
    var quadra = await useCase.ExecutarAsync(id);
    return quadra is null ? Results.NotFound() : Results.Ok(quadra);
});

app.MapPost("/api/quadras", async (CriarQuadraDto dto, CriarQuadraUseCase useCase) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nome))
        return Results.BadRequest(new { mensagem = "Informe o nome da quadra." });

    var resultado = await useCase.ExecutarAsync(dto);
    return Results.Created($"/api/quadras/{resultado.Id}", resultado);
}).RequireAuthorization("Admin");

app.MapPut("/api/quadras/{id:guid}", async (Guid id, AtualizarQuadraDto dto, AtualizarQuadraUseCase useCase) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nome))
        return Results.BadRequest(new { mensagem = "Informe o nome da quadra." });

    var sucesso = await useCase.ExecutarAsync(id, dto);
    return sucesso ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("Admin");

app.MapDelete("/api/quadras/{id:guid}", async (Guid id, DesativarQuadraUseCase useCase) =>
{
    var sucesso = await useCase.ExecutarAsync(id);
    return sucesso ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("Admin");

// --- Endpoints de Atletas ---
app.MapPost("/api/auth/registro", async (
    CriarAtletaDto dto,
    CriarAtletaUseCase createUseCase,
    JwtTokenService tokens,
    IAtletaRepository atletas) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email)
        || string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 8)
    {
        return Results.BadRequest(new { mensagem = "Informe nome, e-mail e uma senha com pelo menos 8 caracteres." });
    }

    try
    {
        var created = await createUseCase.ExecutarAsync(dto);
        var atleta = await atletas.ObterPorIdAsync(created.Id)
            ?? throw new InvalidOperationException("O atleta recém-criado não foi encontrado.");
        return Results.Ok(tokens.CriarToken(atleta));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { mensagem = ex.Message });
    }
});

app.MapPost("/api/auth/login", async (
    EntrarDto dto,
    AutenticarAtletaUseCase authenticateUseCase,
    JwtTokenService tokens) =>
{
    var atleta = await authenticateUseCase.ExecutarAsync(dto.Email, dto.Senha);
    return atleta is null
        ? Results.Json(new { mensagem = "E-mail ou senha incorretos." }, statusCode: StatusCodes.Status401Unauthorized)
        : Results.Ok(tokens.CriarToken(atleta));
});

app.MapGet("/api/atletas", async (ListarAtletasUseCase useCase) =>
    Results.Ok(await useCase.ExecutarAsync()))
    .RequireAuthorization("Admin");

app.MapGet("/api/atletas/{id:guid}", async (Guid id, ObterAtletaUseCase useCase) =>
{
    var atleta = await useCase.ExecutarAsync(id);
    return atleta is null ? Results.NotFound() : Results.Ok(atleta);
}).RequireAuthorization("Admin");

app.MapPost("/api/atletas", async (CriarAtletaDto dto, CriarAtletaUseCase useCase) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email)
        || string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 8)
    {
        return Results.BadRequest(new { mensagem = "Informe nome, e-mail e uma senha com pelo menos 8 caracteres." });
    }

    try
    {
        var resultado = await useCase.ExecutarAsync(dto);
        return Results.Created($"/api/atletas/{resultado.Id}", resultado);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { mensagem = ex.Message });
    }
});

app.MapPut("/api/atletas/{id:guid}", async (Guid id, AtualizarAtletaDto dto, AtualizarAtletaUseCase useCase) =>
{
    var sucesso = await useCase.ExecutarAsync(id, dto);
    return sucesso ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("Admin");

// --- Endpoints de Reservas ---
app.MapPost("/api/reservas", async (
    AgendarReservaRequestDto dto,
    ClaimsPrincipal user,
    AgendarReservaUseCase useCase) =>
{
    if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var atletaId))
        return Results.Unauthorized();

    var inicio = dto.Inicio.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(dto.Inicio, DateTimeKind.Utc)
        : dto.Inicio.ToUniversalTime();
    var fim = dto.Fim.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(dto.Fim, DateTimeKind.Utc)
        : dto.Fim.ToUniversalTime();
    if (inicio <= DateTime.UtcNow || fim <= inicio)
        return Results.BadRequest(new { mensagem = "A reserva deve começar no futuro e terminar depois do início." });

    try
    {
        var resultado = await useCase.ExecutarAsync(new CriarReservaDto(
            dto.QuadraId,
            atletaId,
            inicio,
            fim));
        return Results.Created($"/api/reservas/{resultado.Id}", resultado);
    }
    catch (ReservaConflitanteException ex)
    {
        return Results.Conflict(new { mensagem = ex.Message });
    }
}).RequireAuthorization("Athlete");

app.MapGet("/api/reservas/minhas", async (ClaimsPrincipal user, IReservaRepository repository) =>
{
    if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var atletaId))
        return Results.Unauthorized();

    var reservas = await repository.ListarPorAtletaAsync(atletaId);
    return Results.Ok(reservas.Select(reserva => new ReservaResponseDto(
        reserva.Id,
        reserva.QuadraId,
        reserva.AtletaId,
        reserva.DataHoraInicio,
        reserva.DataHoraFim,
        reserva.ValorCobrado,
        reserva.Status.ToString())));
}).RequireAuthorization("Athlete");

app.MapDelete("/api/reservas/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    IReservaRepository repository,
    CancelarReservaUseCase useCase) =>
{
    var reserva = await repository.ObterPorIdAsync(id);
    if (reserva is null)
        return Results.NotFound();

    var isAdmin = user.IsInRole("Admin");
    if (!isAdmin && (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var atletaId)
        || reserva.AtletaId != atletaId))
    {
        return Results.Forbid();
    }

    await useCase.ExecutarAsync(id);
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/api/reservas/semana", async (DateTime inicio, IReservaRepository repo) =>
{
    var fim = inicio.AddDays(7);
    var reservas = await repo.ListarPorSemanaAsync(inicio, fim);
    return Results.Ok(reservas.Select(reserva => new DisponibilidadeReservaDto(
        reserva.QuadraId,
        reserva.DataHoraInicio,
        reserva.DataHoraFim,
        reserva.Status.ToString())));
});

app.Run();