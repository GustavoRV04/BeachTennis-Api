using BeachTennis.Domain.Entities;

namespace BeachTennis.Domain.Interfaces;

public interface IAtletaRepository
{
    Task<Atleta?> ObterPorIdAsync(Guid id);
    Task<Atleta?> ObterPorEmailAsync(string email);
    Task<List<Atleta>> ListarAsync();
    Task AdicionarAsync(Atleta atleta);
    Task SalvarAsync();
}