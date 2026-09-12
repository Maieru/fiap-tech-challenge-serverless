using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public sealed class OrdemServicoAccessRepository : IOrdemServicoAccessRepository
{
    private readonly Func<CancellationToken, Task<AuthorizationDbContext>> _contextFactory;

    public OrdemServicoAccessRepository(IConnectionStringProvider connectionStringProvider)
    {
        var contextFactory = new AuthorizationDbContextFactory(connectionStringProvider);
        _contextFactory = contextFactory.CreateAsync;
    }

    internal OrdemServicoAccessRepository(Func<CancellationToken, Task<AuthorizationDbContext>> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<bool> HasAccessAsync(Guid ordemServicoId, string accessToken, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory(cancellationToken);

        var authorizationData = await context.OrdensServico.AsNoTracking()
            .Where(ordemServico => ordemServico.Id == ordemServicoId)
            .Select(ordemServico => new
            {
                ordemServico.CodigoAprovacao,
                ordemServico.Cliente.Cpf
            })
            .SingleOrDefaultAsync(cancellationToken);

        return authorizationData is not null &&
            Cpf.TryCreate(authorizationData.Cpf, out var cpf) &&
            CpfAccessToken.Matches(cpf, authorizationData.CodigoAprovacao, accessToken);
    }
}
