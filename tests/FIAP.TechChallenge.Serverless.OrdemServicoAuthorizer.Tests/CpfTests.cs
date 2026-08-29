using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class CpfTests
{
    [TestCase("52998224725")]
    [TestCase("529.982.247-25")]
    public void TryCreate_ShouldCreateNormalizedCpf_WhenCpfIsValid(string input)
    {
        var result = Cpf.TryCreate(input, out var cpf);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(cpf, Is.Not.Null);
            Assert.That(cpf!.Value, Is.EqualTo("52998224725"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("11111111111")]
    [TestCase("52998224724")]
    [TestCase("123")]
    public void TryCreate_ShouldReturnFalse_WhenCpfIsInvalid(string? input)
    {
        var result = Cpf.TryCreate(input, out var cpf);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(cpf, Is.Null);
        });
    }
}
