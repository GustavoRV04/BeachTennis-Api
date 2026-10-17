using BeachTennis.Domain.Entities;

namespace BeachTennis.Domain.Interfaces;

public interface IQuadraRepository
{
    Task<Quadra?> ObterPorIdAsync(Guid id);
    Task<List<Quadra>> ListarAtivasAsync();
    Task<List<Quadra>> ListarTodasAsync();
    Task AdicionarAsync(Quadra quadra);
    Task SalvarAsync();
}