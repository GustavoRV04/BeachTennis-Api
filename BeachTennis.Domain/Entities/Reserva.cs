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

    public Reserva(Guid quadraId, Guid atletaId, DateTime inicio, DateTime fim, decimal valorCobrado)
    {
        if (fim <= inicio)
            throw new ArgumentException("O horário final deve ser depois do inicial.");

        Id = Guid.NewGuid();
        QuadraId = quadraId;
        AtletaId = atletaId;
        DataHoraInicio = inicio;
        DataHoraFim = fim;
        Status = StatusReserva.Confirmada;
        if (valorCobrado < 0)
            throw new ArgumentOutOfRangeException(nameof(valorCobrado));

        ValorCobrado = valorCobrado;
    }

    public void Cancelar() => Status = StatusReserva.Cancelada;
}