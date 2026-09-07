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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Endpoints de Quadras ---
app.MapGet("/api/quadras", async (IQuadraRepository repo) =>
    Results.Ok(await repo.ListarAtivasAsync()));

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