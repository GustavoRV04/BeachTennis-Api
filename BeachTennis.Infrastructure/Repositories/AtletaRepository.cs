using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;
using BeachTennis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeachTennis.Infrastructure.Repositories;

public class AtletaRepository : IAtletaRepository
{
    private readonly AppDbContext _context;

    public AtletaRepository(AppDbContext context) => _context = context;

    public async Task<Atleta?> ObterPorIdAsync(Guid id)
        => await _context.Atletas.FirstOrDefaultAsync(a => a.Id == id);

    // ALTERAÇÃO AQUI: Retorna a lista completa de atletas sem o filtro ".Where(a => a.Ativo)"
    public async Task<List<Atleta>> ListarAsync()
        => await _context.Atletas.ToListAsync();

    public Task SalvarAsync() => _context.SaveChangesAsync();

    public async Task AdicionarAsync(Atleta atleta)
        => await _context.Atletas.AddAsync(atleta);

    public async Task<Atleta?> ObterPorEmailAsync(string email)
        => await _context.Atletas.FirstOrDefaultAsync(a => a.Email == email);
}