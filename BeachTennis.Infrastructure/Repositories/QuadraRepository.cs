using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;
using BeachTennis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeachTennis.Infrastructure.Repositories;

public class QuadraRepository : IQuadraRepository
{
    private readonly AppDbContext _context;

    public QuadraRepository(AppDbContext context) => _context = context;

    public Task<Quadra?> ObterPorIdAsync(Guid id)
        => _context.Quadras.FirstOrDefaultAsync(q => q.Id == id);

    public Task<List<Quadra>> ListarAtivasAsync()
        => _context.Quadras.Where(q => q.Ativa).ToListAsync();

    public Task SalvarAsync() => _context.SaveChangesAsync();

    public async Task AdicionarAsync(Quadra quadra)
        => await _context.Quadras.AddAsync(quadra);
}