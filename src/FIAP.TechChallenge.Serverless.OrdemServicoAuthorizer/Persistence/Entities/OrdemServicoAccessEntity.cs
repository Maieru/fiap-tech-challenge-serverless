namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence.Entities;

internal sealed class OrdemServicoAccessEntity
{
    public Guid Id { get; set; }
    public bool Ativo { get; set; }
    public Guid ClienteId { get; set; }
    public ClienteAccessEntity Cliente { get; set; } = null!;
}
