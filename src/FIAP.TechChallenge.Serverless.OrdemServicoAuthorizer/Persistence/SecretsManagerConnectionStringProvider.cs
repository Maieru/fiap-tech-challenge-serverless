using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

public sealed class SecretsManagerConnectionStringProvider(IAmazonSecretsManager secretsManager, string secretId, string secretKey) : IConnectionStringProvider
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _connectionString;

    public async Task<string> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_connectionString is not null)
            return _connectionString;

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_connectionString is not null)
                return _connectionString;

            var response = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId }, cancellationToken);

            _connectionString = ExtractConnectionString(response.SecretString, secretKey);
            return _connectionString;
        }
        finally
        {
            _ = _lock.Release();
        }
    }

    internal static string ExtractConnectionString(string? secretString, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretString))
            throw new InvalidOperationException("O segredo do banco de dados esta vazio.");

        using var document = JsonDocument.Parse(secretString);

        if (!document.RootElement.TryGetProperty(secretKey, out var value) || string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidOperationException($"A chave '{secretKey}' nao foi encontrada no segredo do banco de dados.");

        return value.GetString()!;
    }
}
