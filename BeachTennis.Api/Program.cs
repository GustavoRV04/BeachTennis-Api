using BeachTennis.Application.DTOs;
using BeachTennis.Application.Exceptions;
using BeachTennis.Application.UseCases;
using BeachTennis.Domain.Interfaces;
using BeachTennis.Infrastructure.Persistence;
using BeachTennis.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Endpoints de Quadras ---
app.MapGet("/api/quadras", async (ListarQuadrasUseCase useCase) =>
    Results.Ok(await useCase.ExecutarAsync()));

app.MapGet("/api/quadras/{id:guid}", async (Guid id, ObterQuadraUseCase useCase) =>
{
    var quadra = await useCase.ExecutarAsync(id);
    return quadra is null ? Results.NotFound() : Results.Ok(quadra);
});

app.MapPost("/api/quadras", async (CriarQuadraDto dto, CriarQuadraUseCase useCase) =>
{
    var resultado = await useCase.ExecutarAsync(dto);
    return Results.Created($"/api/quadras/{resultado.Id}", resultado);
});

app.MapPut("/api/quadras/{id:guid}", async (Guid id, AtualizarQuadraDto dto, AtualizarQuadraUseCase useCase) =>
{
    var sucesso = await useCase.ExecutarAsync(id, dto);
    return sucesso ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/api/quadras/{id:guid}", async (Guid id, DesativarQuadraUseCase useCase) =>
{
    var sucesso = await useCase.ExecutarAsync(id);
    return sucesso ? Results.NoContent() : Results.NotFound();
});

// --- Endpoints de Atletas ---
app.MapGet("/api/atletas", async (ListarAtletasUseCase useCase) =>
    Results.Ok(await useCase.ExecutarAsync()));

app.MapGet("/api/atletas/{id:guid}", async (Guid id, ObterAtletaUseCase useCase) =>
{
    var atleta = await useCase.ExecutarAsync(id);
    return atleta is null ? Results.NotFound() : Results.Ok(atleta);
});

app.MapPost("/api/atletas", async (CriarAtletaDto dto, CriarAtletaUseCase useCase) =>
{
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
});

// --- Endpoints de Reservas ---
app.MapPost("/api/reservas", async (CriarReservaDto dto, AgendarReservaUseCase useCase) =>
{
    try
    {
        var resultado = await useCase.ExecutarAsync(dto);
        return Results.Created($"/api/reservas/{resultado.Id}", resultado);
    }
    catch (ReservaConflitanteException ex)
    {
        return Results.Conflict(new { mensagem = ex.Message });
    }
});

app.MapDelete("/api/reservas/{id:guid}", async (Guid id, CancelarReservaUseCase useCase) =>
{
    await useCase.ExecutarAsync(id);
    return Results.NoContent();
});

app.MapGet("/api/reservas/semana", async (DateTime inicio, IReservaRepository repo) =>
{
    var fim = inicio.AddDays(7);
    return Results.Ok(await repo.ListarPorSemanaAsync(inicio, fim));
});

app.Run();