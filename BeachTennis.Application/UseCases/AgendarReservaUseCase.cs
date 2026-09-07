using BeachTennis.Application.DTOs;
using BeachTennis.Application.Exceptions;
using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Application.UseCases;

public class AgendarReservaUseCase
{
    private readonly IReservaRepository _reservaRepository;

    public AgendarReservaUseCase(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task<ReservaResponseDto> ExecutarAsync(CriarReservaDto dto)
    {
        var conflito = await _reservaRepository.ExisteConflitoAsync(
            dto.QuadraId, dto.Inicio, dto.Fim);

        if (conflito)
            throw new ReservaConflitanteException();

        var reserva = new Reserva(dto.QuadraId, dto.AtletaId, dto.Inicio, dto.Fim);

        await _reservaRepository.AdicionarAsync(reserva);
        await _reservaRepository.SalvarAsync();

        return new ReservaResponseDto(
            reserva.Id, reserva.QuadraId, reserva.AtletaId,
            reserva.DataHoraInicio, reserva.DataHoraFim,
            reserva.ValorCobrado, reserva.Status.ToString());
    }
}