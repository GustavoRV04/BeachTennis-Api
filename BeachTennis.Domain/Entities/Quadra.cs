namespace BeachTennis.Domain.Entities;

public class Quadra
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public bool Ativa { get; private set; } = true;
    public string Descricao { get; private set; } = string.Empty;
    public string Cobertura { get; private set; } = "Ao ar livre";
    public string[] Comodidades { get; private set; } = [];
    public string? ImagemUrl { get; private set; }
    public List<HorarioQuadra> Horarios { get; private set; } = [];

    public void AtualizarDados(
        string nome,
        string descricao,
        string cobertura,
        string[] comodidades,
        string? imagemUrl,
        IEnumerable<HorarioQuadra> horarios)
    {
        Nome = nome;
        Descricao = descricao;
        Cobertura = cobertura;
        Comodidades = comodidades;
        ImagemUrl = imagemUrl;

        var horariosAtualizados = horarios.ToList();
        foreach (var horarioAtualizado in horariosAtualizados)
        {
            var horarioExistente = Horarios.SingleOrDefault(
                horario => horario.DiaSemana == horarioAtualizado.DiaSemana);
            if (horarioExistente is null)
            {
                Horarios.Add(horarioAtualizado);
            }
            else
            {
                horarioExistente.Atualizar(
                    horarioAtualizado.Aberta,
                    horarioAtualizado.AbreAs,
                    horarioAtualizado.FechaAs,
                    horarioAtualizado.ValorHoraAntesDasDezoito,
                    horarioAtualizado.ValorHoraAposDezoito);
            }
        }

        Horarios.RemoveAll(horario =>
            horariosAtualizados.All(atualizado => atualizado.DiaSemana != horario.DiaSemana));
    }

    public void Ativar() => Ativa = true;

    protected Quadra() { } // EF Core

    public Quadra(
        string nome,
        string descricao,
        string cobertura,
        string[] comodidades,
        string? imagemUrl,
        IEnumerable<HorarioQuadra> horarios)
    {
        Id = Guid.NewGuid();
        AtualizarDados(nome, descricao, cobertura, comodidades, imagemUrl, horarios);
    }

    public void Desativar() => Ativa = false;
}