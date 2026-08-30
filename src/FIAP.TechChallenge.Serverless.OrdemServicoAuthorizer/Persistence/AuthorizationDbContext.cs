using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

internal sealed class AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options) : DbContext(options)
{
    public DbSet<ClienteAccessEntity> Clientes => Set<ClienteAccessEntity>();
    public DbSet<OrdemServicoAccessEntity> OrdensServico => Set<OrdemServicoAccessEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var cliente = modelBuilder.Entity<ClienteAccessEntity>();
        _ = cliente.ToTable("Clientes");
        _ = cliente.HasKey(entity => entity.Id);
        _ = cliente.Property(entity => entity.Cpf).HasMaxLength(14);
        _ = cliente.HasQueryFilter(entity => entity.Ativo);

        var ordemServico = modelBuilder.Entity<OrdemServicoAccessEntity>();
        _ = ordemServico.ToTable("OrdensServico");
        _ = ordemServico.HasKey(entity => entity.Id);
        _ = ordemServico.Property(entity => entity.CodigoAprovacao).IsRequired();
        _ = ordemServico.HasQueryFilter(entity => entity.Ativo);
        _ = ordemServico
            .HasOne(entity => entity.Cliente)
            .WithMany()
            .HasForeignKey(entity => entity.ClienteId)
            .IsRequired();
    }
}
