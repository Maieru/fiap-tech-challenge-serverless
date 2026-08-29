using System.Text.Json.Serialization;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;

public sealed class HttpApiAuthorizerRequest
{
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    [JsonPropertyName("pathParameters")]
    public Dictionary<string, string>? PathParameters { get; init; }
}
