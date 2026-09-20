using BeachTennis.Application.DTOs;
using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Application.UseCases;

public class CriarAtletaUseCase
{
    private readonly IAtletaRepository _repo;
    public CriarAtletaUseCase(IAtletaRepository repo) => _repo = repo;

    public async Task<AtletaResponseDto> ExecutarAsync(CriarAtletaDto dto)
    {
        var existente = await _repo.ObterPorEmailAsync(dto.Email);
        if (existente is not null)
            throw new InvalidOperationException("Já existe um atleta cadastrado com este e-mail.");

        // Hash simples por enquanto — trocar por BCrypt/Identity quando entrar o login (JWT)
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var atleta = new Atleta(dto.Nome, dto.Email, senhaHash);
        await _repo.AdicionarAsync(atleta);
        await _repo.SalvarAsync();

        return new AtletaResponseDto(atleta.Id, atleta.Nome, atleta.Email);
    }
}

public class ListarAtletasUseCase
{
    private readonly IAtletaRepository _repo;
    public ListarAtletasUseCase(IAtletaRepository repo) => _repo = repo;

    public async Task<List<AtletaResponseDto>> ExecutarAsync()
    {
        var atletas = await _repo.ListarAsync();
        return atletas.Select(a => new AtletaResponseDto(a.Id, a.Nome, a.Email)).ToList();
    }
}

public class ObterAtletaUseCase
{
    private readonly IAtletaRepository _repo;
    public ObterAtletaUseCase(IAtletaRepository repo) => _repo = repo;

    public async Task<AtletaResponseDto?> ExecutarAsync(Guid id)
    {
        var atleta = await _repo.ObterPorIdAsync(id);
        return atleta is null ? null : new AtletaResponseDto(atleta.Id, atleta.Nome, atleta.Email);
    }
}

public class AtualizarAtletaUseCase
{
    private readonly IAtletaRepository _repo;
    public AtualizarAtletaUseCase(IAtletaRepository repo) => _repo = repo;

    public async Task<bool> ExecutarAsync(Guid id, AtualizarAtletaDto dto)
    {
        var atleta = await _repo.ObterPorIdAsync(id);
        if (atleta is null) return false;

        atleta.AtualizarDados(dto.Nome, dto.Email);
        await _repo.SalvarAsync();
        return true;
    }
}