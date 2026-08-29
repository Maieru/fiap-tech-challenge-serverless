namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence.Entities;

internal sealed class ClienteAccessEntity
{
    public Guid Id { get; set; }
    public bool Ativo { get; set; }
    public string? Cpf { get; set; }
}
