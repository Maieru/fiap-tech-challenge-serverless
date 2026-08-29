using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests")]
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]