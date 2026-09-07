namespace BeachTennis.Application.DTOs;

public record CriarReservaDto(Guid QuadraId, Guid AtletaId, DateTime Inicio, DateTime Fim);

public record ReservaResponseDto(
    Guid Id, Guid QuadraId, Guid AtletaId,
    DateTime Inicio, DateTime Fim, decimal Valor, string Status);