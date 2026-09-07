using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Application.UseCases;

public class CancelarReservaUseCase
{
    private readonly IReservaRepository _reservaRepository;

    public CancelarReservaUseCase(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task ExecutarAsync(Guid reservaId)
    {
        var reserva = await _reservaRepository.ObterPorIdAsync(reservaId)
            ?? throw new KeyNotFoundException("Reserva não encontrada.");

        reserva.Cancelar();
        await _reservaRepository.SalvarAsync();
    }
}