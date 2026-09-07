namespace BeachTennis.Domain.Entities;

public class Quadra
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public bool Ativa { get; private set; } = true;

    protected Quadra() { } // EF Core

    public Quadra(string nome)
    {
        Id = Guid.NewGuid();
        Nome = nome;
    }

    public void Desativar() => Ativa = false;
}