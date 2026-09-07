using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Enums;
using BeachTennis.Domain.Interfaces;
using BeachTennis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BeachTennis.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context) => _context = context;

    public Task<Reserva?> ObterPorIdAsync(Guid id)
        => _context.Reservas.FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Reserva>> ListarPorSemanaAsync(DateTime inicioSemana, DateTime fimSemana)
        => _context.Reservas
            .Where(r => r.DataHoraInicio >= inicioSemana && r.DataHoraInicio < fimSemana)
            .ToListAsync();

    public Task<bool> ExisteConflitoAsync(Guid quadraId, DateTime inicio, DateTime fim)
        => _context.Reservas.AnyAsync(r =>
            r.QuadraId == quadraId &&
            r.Status == StatusReserva.Confirmada &&
            r.DataHoraInicio < fim &&
            inicio < r.DataHoraFim); // sobreposição de intervalos

    public async Task AdicionarAsync(Reserva reserva)
        => await _context.Reservas.AddAsync(reserva);

    public Task SalvarAsync() => _context.SaveChangesAsync();
}