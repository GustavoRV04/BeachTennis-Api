using BeachTennis.Domain.Entities;
using BeachTennis.Domain.Interfaces;

namespace BeachTennis.Api.Security;

public class AdminProvisioner(IConfiguration configuration, IAtletaRepository atletas)
{
    public async Task ProvisionarAsync()
    {
        var email = configuration["Admin:Email"]?.Trim().ToLowerInvariant();
        var senha = configuration["Admin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) || senha.Length < 12)
        {
            throw new InvalidOperationException(
                "Configure Admin:Email e Admin:Password (mínimo de 12 caracteres) usando User Secrets ou os segredos do host.");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(senha);
        var admin = await atletas.ObterPorEmailAsync(email);
        if (admin is null)
        {
            await atletas.AdicionarAsync(new Atleta("Administrador", email, hash, isAdmin: true));
        }
        else
        {
            admin.ConfigurarAdministrador(hash);
        }

        await atletas.SalvarAsync();
    }
}

public class AdminProvisioningService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var provisioner = scope.ServiceProvider.GetRequiredService<AdminProvisioner>();
        await provisioner.ProvisionarAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
