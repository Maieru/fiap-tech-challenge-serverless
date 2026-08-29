namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public sealed class FixedConnectionStringProvider(string connectionString) : IConnectionStringProvider
{
    public Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(connectionString);
    }
}
