using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public sealed class EntityFrameworkOrdemServicoAccessRepository : IOrdemServicoAccessRepository
{
    private readonly Func<CancellationToken, Task<AuthorizationDbContext>> _contextFactory;

    public EntityFrameworkOrdemServicoAccessRepository(IConnectionStringProvider connectionStringProvider)
    {
        var contextFactory = new AuthorizationDbContextFactory(connectionStringProvider);
        _contextFactory = contextFactory.CreateAsync;
    }

    internal EntityFrameworkOrdemServicoAccessRepository(Func<CancellationToken, Task<AuthorizationDbContext>> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> HasAccessAsync(Guid ordemServicoId, Cpf cpf, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory(cancellationToken);

        return await context.OrdensServico.AsNoTracking()
            .AnyAsync(
                ordemServico => ordemServico.Id == ordemServicoId && ordemServico.Cliente.Cpf == cpf.Value,
                cancellationToken);
    }
}
