using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class SecretsManagerConnectionStringProviderTests
{
    private const string SecretKey = "ConnectionStrings__DefaultConnection";

    [Test]
    public void ExtractConnectionString_ShouldReturnValue_WhenSecretContainsExpectedKey()
    {
        const string expected = "Host=database;Port=5432;Database=fiap;Username=user;Password=password";
        var secret = $$"""
            {
              "{{SecretKey}}": "{{expected}}"
            }
            """;

        var result = SecretsManagerConnectionStringProvider.ExtractConnectionString(secret, SecretKey);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("{}")]
    public void ExtractConnectionString_ShouldThrow_WhenSecretIsMissingOrInvalid(string? secret)
    {
        Assert.That(
            () => SecretsManagerConnectionStringProvider.ExtractConnectionString(secret, SecretKey),
            Throws.TypeOf<InvalidOperationException>());
    }
}
