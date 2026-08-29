using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public interface IOrdemServicoAccessRepository
{
    Task<bool> HasAccessAsync(Guid ordemServicoId, Cpf cpf, CancellationToken cancellationToken = default);
}
