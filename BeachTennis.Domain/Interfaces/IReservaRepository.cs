using BeachTennis.Domain.Entities;

namespace BeachTennis.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> ObterPorIdAsync(Guid id);
    Task<List<Reserva>> ListarPorSemanaAsync(DateTime inicioSemana, DateTime fimSemana);
    Task<List<Reserva>> ListarPorAtletaAsync(Guid atletaId);

    // Usado para checar conflito de horário antes de confirmar a reserva
    Task<bool> ExisteConflitoAsync(Guid quadraId, DateTime inicio, DateTime fim);

    Task AdicionarAsync(Reserva reserva);
    Task SalvarAsync(); // commit das alterações (Unit of Work simplificado)
}