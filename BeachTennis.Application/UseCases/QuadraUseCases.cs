using BeachTennis.Application.DTOs;
using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Application.UseCases;

public class CriarQuadraUseCase
{
    private readonly IQuadraRepository _repo;
    public CriarQuadraUseCase(IQuadraRepository repo) => _repo = repo;

    public async Task<QuadraResponseDto> ExecutarAsync(CriarQuadraDto dto)
    {
        var horarios = dto.Horarios.Select(horario => new HorarioQuadra(
            horario.DiaSemana,
            horario.Aberta,
            horario.AbreAs,
            horario.FechaAs,
            horario.ValorHoraAntesDasDezoito,
            horario.ValorHoraAposDezoito));
        var quadra = new Quadra(dto.Nome, dto.Descricao, dto.Cobertura, dto.Comodidades, dto.ImagemUrl, horarios);
        await _repo.AdicionarAsync(quadra);
        await _repo.SalvarAsync();
        return QuadraResponseDto.From(quadra);
    }
}

public class ListarQuadrasUseCase
{
    private readonly IQuadraRepository _repo;
    public ListarQuadrasUseCase(IQuadraRepository repo) => _repo = repo;

    public async Task<List<QuadraResponseDto>> ExecutarAsync()
    {
        var quadras = await _repo.ListarAtivasAsync();
        return quadras.Select(QuadraResponseDto.From).ToList();
    }
}

public class ObterQuadraUseCase
{
    private readonly IQuadraRepository _repo;
    public ObterQuadraUseCase(IQuadraRepository repo) => _repo = repo;

    public async Task<QuadraResponseDto?> ExecutarAsync(Guid id)
    {
        var quadra = await _repo.ObterPorIdAsync(id);
        return quadra is null ? null : QuadraResponseDto.From(quadra);
    }
}

public class AtualizarQuadraUseCase
{
    private readonly IQuadraRepository _repo;
    public AtualizarQuadraUseCase(IQuadraRepository repo) => _repo = repo;

    public async Task<bool> ExecutarAsync(Guid id, AtualizarQuadraDto dto)
    {
        var quadra = await _repo.ObterPorIdAsync(id);
        if (quadra is null) return false;

        var horarios = dto.Horarios.Select(horario => new HorarioQuadra(
            horario.DiaSemana,
            horario.Aberta,
            horario.AbreAs,
            horario.FechaAs,
            horario.ValorHoraAntesDasDezoito,
            horario.ValorHoraAposDezoito));
        quadra.AtualizarDados(dto.Nome, dto.Descricao, dto.Cobertura, dto.Comodidades, dto.ImagemUrl, horarios);
        await _repo.SalvarAsync();
        return true;
    }
}

public class DesativarQuadraUseCase
{
    private readonly IQuadraRepository _repo;
    public DesativarQuadraUseCase(IQuadraRepository repo) => _repo = repo;

    public async Task<bool> ExecutarAsync(Guid id)
    {
        var quadra = await _repo.ObterPorIdAsync(id);
        if (quadra is null) return false;

        quadra.Desativar();
        await _repo.SalvarAsync();
        return true;
    }
}