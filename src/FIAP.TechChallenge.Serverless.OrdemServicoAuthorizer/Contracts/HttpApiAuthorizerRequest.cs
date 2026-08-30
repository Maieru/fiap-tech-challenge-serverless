using System.Text.Json.Serialization;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;

public sealed class HttpApiAuthorizerRequest
{
    [JsonPropertyName("queryStringParameters")]
    public Dictionary<string, string>? QueryStringParameters { get; init; }

    [JsonPropertyName("pathParameters")]
    public Dictionary<string, string>? PathParameters { get; init; }
}
