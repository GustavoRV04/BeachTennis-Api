using BeachTennis.Domain.Entities;

namespace BeachTennis.Domain.Interfaces;

public interface IAtletaRepository
{
    Task<Atleta?> ObterPorEmailAsync(string email);
    Task AdicionarAsync(Atleta atleta);
}