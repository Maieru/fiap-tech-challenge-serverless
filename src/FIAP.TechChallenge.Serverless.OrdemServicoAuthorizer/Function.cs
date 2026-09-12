using Amazon.SecretsManager;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer;

public sealed class Function
{
    private const string AccessTokenQueryParameterName = "token";
    private const string OrdemServicoIdPathParameterName = "id";
    private const string DatabaseSecretIdEnvironmentVariable = "DATABASE_SECRET_ID";
    private const string DatabaseSecretKey = "ConnectionStrings__DefaultConnection";

    private readonly IOrdemServicoAccessRepository _repository;

    public Function() : this(CreateRepository())
    {
    }

    public Function(IOrdemServicoAccessRepository repository)
    {
        _repository = repository;
    }

    public async Task<HttpApiAuthorizerResponse> FunctionHandler(HttpApiAuthorizerRequest request)
    {
        if (!TryGetValue(request.QueryStringParameters, AccessTokenQueryParameterName, out var accessToken) || !CpfAccessToken.IsWellFormed(accessToken))
            return HttpApiAuthorizerResponse.Deny();

        if (!TryGetValue(request.PathParameters, OrdemServicoIdPathParameterName, out var rawOrdemServicoId) ||
            !Guid.TryParse(rawOrdemServicoId, out var ordemServicoId) ||
            ordemServicoId == Guid.Empty)
        {
            return HttpApiAuthorizerResponse.Deny();
        }

        var hasAccess = await _repository.HasAccessAsync(ordemServicoId, accessToken);

        return hasAccess ? HttpApiAuthorizerResponse.Allow(ordemServicoId) : HttpApiAuthorizerResponse.Deny();
    }

    private static bool TryGetValue(IReadOnlyDictionary<string, string>? values, string key, out string value)
    {
        value = string.Empty;

        if (values is null)
            return false;

        foreach (var pair in values)
        {
            if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                continue;

            value = pair.Value;
            return true;
        }

        return false;
    }

    private static IOrdemServicoAccessRepository CreateRepository()
    {
        var connectionString = Environment.GetEnvironmentVariable(DatabaseSecretKey);

        IConnectionStringProvider connectionStringProvider;

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            connectionStringProvider = new FixedConnectionStringProvider(connectionString);
            return new OrdemServicoAccessRepository(connectionStringProvider);
        }

        var databaseSecretId = Environment.GetEnvironmentVariable(DatabaseSecretIdEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(databaseSecretId))
            throw new InvalidOperationException($"A variavel de ambiente '{DatabaseSecretIdEnvironmentVariable}' deve ser configurada.");

        connectionStringProvider = new SecretsManagerConnectionStringProvider(new AmazonSecretsManagerClient(), databaseSecretId, DatabaseSecretKey);
        return new OrdemServicoAccessRepository(connectionStringProvider);
    }
}
