using BeachTennis.Application.DTOs;
using BeachTennis.Application.Exceptions;
using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Application.UseCases;

public class AgendarReservaUseCase
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IQuadraRepository _quadraRepository;

    private static readonly TimeZoneInfo FusoDaArena = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    public AgendarReservaUseCase(IReservaRepository reservaRepository, IQuadraRepository quadraRepository)
    {
        _reservaRepository = reservaRepository;
        _quadraRepository = quadraRepository;
    }

    public async Task<ReservaResponseDto> ExecutarAsync(CriarReservaDto dto)
    {
        var quadra = await _quadraRepository.ObterPorIdAsync(dto.QuadraId);
        if (quadra is null || !quadra.Ativa)
            throw new ReservaInvalidaException("A quadra selecionada não está disponível para reservas.");

        var inicioLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dto.Inicio, DateTimeKind.Utc), FusoDaArena);
        var fimLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dto.Fim, DateTimeKind.Utc), FusoDaArena);
        var horario = quadra.Horarios.SingleOrDefault(item => item.DiaSemana == inicioLocal.DayOfWeek);
        if (horario is null || !horario.Aberta)
            throw new ReservaInvalidaException("A quadra não abre neste dia.");

        if (dto.Fim - dto.Inicio != TimeSpan.FromMinutes(90))
            throw new ReservaInvalidaException("Cada reserva deve ter duração de 90 minutos.");

        var minutosDesdeAbertura = inicioLocal.TimeOfDay - horario.AbreAs.ToTimeSpan();
        if (minutosDesdeAbertura < TimeSpan.Zero
            || minutosDesdeAbertura.Ticks % TimeSpan.FromMinutes(90).Ticks != 0
            || fimLocal.Date != inicioLocal.Date
            || fimLocal.TimeOfDay > horario.FechaAs.ToTimeSpan())
        {
            throw new ReservaInvalidaException("O horário selecionado está fora do funcionamento da quadra.");
        }

        var valorHora = inicioLocal.Hour < 18
            ? horario.ValorHoraAntesDasDezoito
            : horario.ValorHoraAposDezoito;
        var valorReserva = valorHora * 1.5m;

        var conflito = await _reservaRepository.ExisteConflitoAsync(
            dto.QuadraId, dto.Inicio, dto.Fim);

        if (conflito)
            throw new ReservaConflitanteException();

        var reserva = new Reserva(dto.QuadraId, dto.AtletaId, dto.Inicio, dto.Fim, valorReserva);

        await _reservaRepository.AdicionarAsync(reserva);
        await _reservaRepository.SalvarAsync();

        return new ReservaResponseDto(
            reserva.Id, reserva.QuadraId, reserva.AtletaId,
            reserva.DataHoraInicio, reserva.DataHoraFim,
            reserva.ValorCobrado, reserva.Status.ToString());
    }
}