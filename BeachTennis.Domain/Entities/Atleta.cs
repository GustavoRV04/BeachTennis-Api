namespace BeachTennis.Domain.Entities;

public class Atleta
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;
    public bool IsAdmin { get; private set; }

    public void AtualizarDados(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }

    public void ConfigurarAdministrador(string senhaHash)
    {
        SenhaHash = senhaHash;
        IsAdmin = true;
    }

    protected Atleta() { }

    public Atleta(string nome, string email, string senhaHash, bool isAdmin = false)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        IsAdmin = isAdmin;
    }
}