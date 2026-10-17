namespace BeachTennis.Application.DTOs;

public record EntrarDto(string Email, string Senha);
public record TokenAtletaDto(string Token, DateTime ExpiraEm, AtletaResponseDto Usuario, string Perfil);
