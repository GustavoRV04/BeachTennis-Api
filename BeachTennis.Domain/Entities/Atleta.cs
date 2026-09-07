namespace BeachTennis.Domain.Entities;

public class Atleta
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;

    protected Atleta() { }

    public Atleta(string nome, string email, string senhaHash)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
    }
}