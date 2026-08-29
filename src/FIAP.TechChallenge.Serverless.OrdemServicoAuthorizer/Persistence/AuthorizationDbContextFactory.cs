using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

internal sealed class AuthorizationDbContextFactory(IConnectionStringProvider connectionStringProvider)
{
    private DbContextOptions<AuthorizationDbContext>? _options;

    public async Task<AuthorizationDbContext> CreateAsync(CancellationToken cancellationToken = default)
    {
        if (_options is null)
        {
            var connectionString = await connectionStringProvider.GetAsync(cancellationToken);

            _options = new DbContextOptionsBuilder<AuthorizationDbContext>()
                .UseNpgsql(connectionString, options => options.CommandTimeout(5))
                .Options;
        }

        return new AuthorizationDbContext(_options);
    }
}