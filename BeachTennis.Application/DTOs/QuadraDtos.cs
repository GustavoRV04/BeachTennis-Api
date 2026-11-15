using BeachTennis.Domain.Entities;

namespace BeachTennis.Application.DTOs;

public record HorarioQuadraDto(
    DayOfWeek DiaSemana,
    bool Aberta,
    TimeOnly AbreAs,
    TimeOnly FechaAs,
    decimal ValorHoraAntesDasDezoito,
    decimal ValorHoraAposDezoito);

public record CriarQuadraDto(
    string Nome,
    string Descricao,
    string Cobertura,
    string[] Comodidades,
    string? ImagemUrl,
    List<HorarioQuadraDto> Horarios);

public record AtualizarQuadraDto(
    string Nome,
    string Descricao,
    string Cobertura,
    string[] Comodidades,
    string? ImagemUrl,
    List<HorarioQuadraDto> Horarios);

public record QuadraResponseDto(
    Guid Id,
    string Nome,
    bool Ativa,
    string Descricao,
    string Cobertura,
    string[] Comodidades,
    string? ImagemUrl,
    List<HorarioQuadraDto> Horarios)
{
    public static QuadraResponseDto From(Quadra quadra) => new(
        quadra.Id,
        quadra.Nome,
        quadra.Ativa,
        quadra.Descricao,
        quadra.Cobertura,
        quadra.Comodidades,
        quadra.ImagemUrl,
        quadra.Horarios.OrderBy(horario => horario.DiaSemana)
            .Select(horario => new HorarioQuadraDto(
                horario.DiaSemana,
                horario.Aberta,
                horario.AbreAs,
                horario.FechaAs,
                horario.ValorHoraAntesDasDezoito,
                horario.ValorHoraAposDezoito))
            .ToList());
}