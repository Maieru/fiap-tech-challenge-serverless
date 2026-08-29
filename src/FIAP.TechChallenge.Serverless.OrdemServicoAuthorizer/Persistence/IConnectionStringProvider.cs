namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public interface IConnectionStringProvider
{
    Task<string> GetAsync(CancellationToken cancellationToken = default);
}
