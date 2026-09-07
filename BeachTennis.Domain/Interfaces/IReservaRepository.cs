using BeachTennis.Domain.Entities;

namespace BeachTennis.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> ObterPorIdAsync(Guid id);
    Task<List<Reserva>> ListarPorSemanaAsync(DateTime inicioSemana, DateTime fimSemana);

    // Usado para checar conflito de horário antes de confirmar a reserva
    Task<bool> ExisteConflitoAsync(Guid quadraId, DateTime inicio, DateTime fim);

    Task AdicionarAsync(Reserva reserva);
    Task SalvarAsync(); // commit das alterações (Unit of Work simplificado)
}