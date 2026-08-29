using System.Text.Json.Serialization;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;

public sealed class HttpApiAuthorizerResponse
{
    [JsonPropertyName("isAuthorized")]
    public bool IsAuthorized { get; init; }

    [JsonPropertyName("context")]
    public Dictionary<string, object> Context { get; init; } = [];

    public static HttpApiAuthorizerResponse Allow(Guid ordemServicoId)
    {
        return new HttpApiAuthorizerResponse
        {
            IsAuthorized = true,
            Context = new Dictionary<string, object>
            {
                ["ordemServicoId"] = ordemServicoId.ToString()
            }
        };
    }

    public static HttpApiAuthorizerResponse Deny()
    {
        return new HttpApiAuthorizerResponse { IsAuthorized = false };
    }
}
