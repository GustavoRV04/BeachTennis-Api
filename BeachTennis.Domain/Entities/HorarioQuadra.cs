namespace BeachTennis.Domain.Entities;

public class HorarioQuadra
{
    public Guid QuadraId { get; private set; }
    public DayOfWeek DiaSemana { get; private set; }
    public bool Aberta { get; private set; }
    public TimeOnly AbreAs { get; private set; }
    public TimeOnly FechaAs { get; private set; }
    public decimal ValorHoraAntesDasDezoito { get; private set; }
    public decimal ValorHoraAposDezoito { get; private set; }

    protected HorarioQuadra() { }

    public HorarioQuadra(
        DayOfWeek diaSemana,
        bool aberta,
        TimeOnly abreAs,
        TimeOnly fechaAs,
        decimal valorHoraAntesDasDezoito,
        decimal valorHoraApartirDasDezoito)
    {
        DiaSemana = diaSemana;
        Aberta = aberta;
        AbreAs = abreAs;
        FechaAs = fechaAs;
        ValorHoraAntesDasDezoito = valorHoraAntesDasDezoito;
        ValorHoraAposDezoito = valorHoraApartirDasDezoito;
    }

    public void Atualizar(
        bool aberta,
        TimeOnly abreAs,
        TimeOnly fechaAs,
        decimal valorHoraAntesDasDezoito,
        decimal valorHoraAposDezoito)
    {
        Aberta = aberta;
        AbreAs = abreAs;
        FechaAs = fechaAs;
        ValorHoraAntesDasDezoito = valorHoraAntesDasDezoito;
        ValorHoraAposDezoito = valorHoraAposDezoito;
    }
}
