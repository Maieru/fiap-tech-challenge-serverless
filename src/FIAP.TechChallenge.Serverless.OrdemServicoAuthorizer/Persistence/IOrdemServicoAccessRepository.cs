namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public interface IOrdemServicoAccessRepository
{
    Task<bool> HasAccessAsync(Guid ordemServicoId, string accessToken, CancellationToken cancellationToken = default);
}
