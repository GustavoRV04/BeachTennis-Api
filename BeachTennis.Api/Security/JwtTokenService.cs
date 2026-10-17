using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeachTennis.Application.DTOs;
using BeachTennis.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BeachTennis.Api.Security;

public class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _signingCredentials;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _issuer = configuration["Auth:Issuer"] ?? "BeachTennis.Api";
        _audience = configuration["Auth:Audience"] ?? "BeachTennis.Front";
        var signingKey = configuration["Auth:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException(
                "Configure Auth:SigningKey com pelo menos 32 bytes usando User Secrets ou um segredo no host.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        _expirationMinutes = configuration.GetValue<int?>("Auth:ExpirationMinutes") ?? 60;
        if (_expirationMinutes is < 5 or > 1440)
            throw new InvalidOperationException("Auth:ExpirationMinutes deve estar entre 5 e 1440.");
    }

    public TokenAtletaDto CriarToken(Atleta atleta)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_expirationMinutes);
        var perfil = atleta.IsAdmin ? "Admin" : "Athlete";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, atleta.Id.ToString()),
            new Claim(ClaimTypes.Name, atleta.Nome),
            new Claim(ClaimTypes.Email, atleta.Email),
            new Claim(ClaimTypes.Role, perfil),
        };
        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: expiraEm,
            signingCredentials: _signingCredentials);

        return new TokenAtletaDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiraEm,
            new AtletaResponseDto(atleta.Id, atleta.Nome, atleta.Email),
            perfil);
    }
}
