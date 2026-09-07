namespace BeachTennis.Application.Exceptions;

public class ReservaConflitanteException : Exception
{
    public ReservaConflitanteException()
        : base("Já existe uma reserva confirmada para esta quadra neste horário.") { }
}