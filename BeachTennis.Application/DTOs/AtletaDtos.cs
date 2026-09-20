namespace BeachTennis.Application.DTOs;

public record CriarAtletaDto(string Nome, string Email, string Senha);
public record AtualizarAtletaDto(string Nome, string Email);
public record AtletaResponseDto(Guid Id, string Nome, string Email);