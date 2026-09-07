using BeachTennis.Domain.Enums;

namespace BeachTennis.Domain.Entities;

public class Reserva
{
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

    // Regra de negócio do ADR: R$ 100 antes das 18h, R$ 120 depois das 18h
    private static decimal CalcularValor(DateTime inicio)
        => inicio.Hour < 18 ? 100m : 120m;

    public void Cancelar() => Status = StatusReserva.Cancelada;
}