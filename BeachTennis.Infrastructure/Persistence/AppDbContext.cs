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
            b.Property(q => q.Descricao).HasMaxLength(1000).HasDefaultValue(string.Empty);
            b.Property(q => q.Cobertura).IsRequired().HasMaxLength(30).HasDefaultValue("Ao ar livre");
            b.Property(q => q.Comodidades).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            b.Property(q => q.ImagemUrl).HasMaxLength(2048);
            b.HasMany(q => q.Horarios)
                .WithOne()
                .HasForeignKey(horario => horario.QuadraId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HorarioQuadra>(b =>
        {
            b.HasKey(horario => new { horario.QuadraId, horario.DiaSemana });
            b.Property(horario => horario.DiaSemana).HasConversion<int>();
            b.Property(horario => horario.AbreAs).HasColumnType("time without time zone");
            b.Property(horario => horario.FechaAs).HasColumnType("time without time zone");
            b.Property(horario => horario.ValorHoraAntesDasDezoito).HasColumnType("decimal(10,2)");
            b.Property(horario => horario.ValorHoraAposDezoito).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Atleta>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Email).IsRequired();
            b.Property(a => a.IsAdmin).HasDefaultValue(false);
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