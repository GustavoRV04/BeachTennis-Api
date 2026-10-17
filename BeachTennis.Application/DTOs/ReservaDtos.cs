namespace BeachTennis.Application.DTOs;

public record CriarReservaDto(Guid QuadraId, Guid AtletaId, DateTime Inicio, DateTime Fim);

public record ReservaResponseDto(
    Guid Id, Guid QuadraId, Guid AtletaId,
    DateTime Inicio, DateTime Fim, decimal Valor, string Status);

public record DisponibilidadeReservaDto(Guid QuadraId, DateTime Inicio, DateTime Fim, string Status);

public record AgendarReservaRequestDto(Guid QuadraId, DateTime Inicio, DateTime Fim);