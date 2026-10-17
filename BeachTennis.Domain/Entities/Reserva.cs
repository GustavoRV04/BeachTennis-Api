using BeachTennis.Domain.Enums;

namespace BeachTennis.Domain.Entities;

public class Reserva
{
    private static readonly TimeZoneInfo FusoDaArena = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    public Guid Id { get; private set; }
    public Guid QuadraId { get; private set; }
    public Guid AtletaId { get; private set; }
    public DateTime DataHoraInicio { get; private set; }
    public DateTime DataHoraFim { get; private set; }
    public decimal ValorCobrado { get; private set; }
    public StatusReserva Status { get; private set; }

    protected Reserva() { }

    public Reserva(Guid quadraId, Guid atletaId, DateTime inicio, DateTime fim)
    {
        if (fim <= inicio)
            throw new ArgumentException("O horário final deve ser depois do inicial.");

        Id = Guid.NewGuid();
        QuadraId = quadraId;
        AtletaId = atletaId;
        DataHoraInicio = inicio;
        DataHoraFim = fim;
        Status = StatusReserva.Confirmada;
        ValorCobrado = CalcularValor(inicio);
    }

    // Os horários recebidos pela API são instantes UTC; a regra comercial usa o horário local da arena.
    private static decimal CalcularValor(DateTime inicio)
    {
        var horarioLocal = inicio.Kind == DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(inicio, DateTimeKind.Utc), FusoDaArena)
            : TimeZoneInfo.ConvertTime(inicio, FusoDaArena);

        return horarioLocal.Hour < 18 ? 100m : 120m;
    }

    public void Cancelar() => Status = StatusReserva.Cancelada;
}