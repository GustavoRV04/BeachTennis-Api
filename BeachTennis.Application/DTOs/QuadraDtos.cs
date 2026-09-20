namespace BeachTennis.Application.DTOs;

public record CriarQuadraDto(string Nome);
public record AtualizarQuadraDto(string Nome);
public record QuadraResponseDto(Guid Id, string Nome, bool Ativa);