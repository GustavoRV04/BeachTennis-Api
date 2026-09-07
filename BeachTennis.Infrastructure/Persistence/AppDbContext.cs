using BeachTennis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachTennis.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Quadra> Quadras => Set<Quadra>();
    public DbSet<Atleta> Atletas => Set<Atleta>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quadra>(b =>
        {
            b.HasKey(q => q.Id);
            b.Property(q => q.Nome).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Atleta>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Email).IsRequired();
            b.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Reserva>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.ValorCobrado).HasColumnType("decimal(10,2)");

            // Índice composto: ajuda a checar conflitos por quadra/horário
            b.HasIndex(r => new { r.QuadraId, r.DataHoraInicio, r.DataHoraFim });
        });

        base.OnModelCreating(modelBuilder);
    }
}