using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class CpfAccessTokenTests
{
    [Test]
    public void Create_ShouldReturnSha256FromNormalizedCpfAndCodigoAprovacao()
    {
        _ = Cpf.TryCreate("529.982.247-25", out var cpf);
        var codigoAprovacao = Guid.Parse("79ee2120-ab05-4cba-a57d-011d654248dd");

        var token = CpfAccessToken.Create(cpf!, codigoAprovacao);

        Assert.That(token, Is.EqualTo("d80e8c958d265c7455badf562f4cbd7914a38bda2698edb1cff74357e9bbc2d6"));
    }

    [Test]
    public void Matches_ShouldReturnTrueIgnoringHexCase()
    {
        _ = Cpf.TryCreate("52998224725", out var cpf);
        var codigoAprovacao = Guid.Parse("79ee2120-ab05-4cba-a57d-011d654248dd");
        var token = CpfAccessToken.Create(cpf!, codigoAprovacao);

        var result = CpfAccessToken.Matches(cpf!, codigoAprovacao, token.ToUpperInvariant());

        Assert.That(result, Is.True);
    }
}
