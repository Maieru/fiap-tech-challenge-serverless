using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class FunctionTests
{
    private static readonly Guid OrdemServicoId = Guid.Parse("9be8b471-bd51-4f44-a0e0-3db12cb342c3");

    [Test]
    public async Task FunctionHandler_ShouldAllowRequest_WhenCpfBelongsToOrdemServico()
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);
        var request = CreateRequest("529.982.247-25", OrdemServicoId.ToString());

        var response = await function.FunctionHandler(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.True);
            Assert.That(response.Context["ordemServicoId"], Is.EqualTo(OrdemServicoId.ToString()));
            Assert.That(repository.CallCount, Is.EqualTo(1));
            Assert.That(repository.LastOrdemServicoId, Is.EqualTo(OrdemServicoId));
            Assert.That(repository.LastCpf?.Value, Is.EqualTo("52998224725"));
        });
    }

    [Test]
    public async Task FunctionHandler_ShouldDenyRequest_WhenCpfDoesNotBelongToOrdemServico()
    {
        var repository = new FakeOrdemServicoAccessRepository(false);
        var function = new Function(repository);

        var response = await function.FunctionHandler(CreateRequest("52998224725", OrdemServicoId.ToString()));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.False);
            Assert.That(response.Context, Is.Empty);
            Assert.That(repository.CallCount, Is.EqualTo(1));
        });
    }

    [TestCase(null)]
    [TestCase("11111111111")]
    [TestCase("52998224724")]
    public async Task FunctionHandler_ShouldDenyWithoutQueryingDatabase_WhenCpfIsMissingOrInvalid(string? cpf)
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);

        var response = await function.FunctionHandler(CreateRequest(cpf, OrdemServicoId.ToString()));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.False);
            Assert.That(repository.CallCount, Is.Zero);
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("nao-e-guid")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public async Task FunctionHandler_ShouldDenyWithoutQueryingDatabase_WhenOrdemServicoIdIsInvalid(string? id)
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);

        var response = await function.FunctionHandler(CreateRequest("52998224725", id));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.False);
            Assert.That(repository.CallCount, Is.Zero);
        });
    }

    [Test]
    public async Task FunctionHandler_ShouldReadCpfHeaderCaseInsensitively()
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);
        var request = new HttpApiAuthorizerRequest
        {
            Headers = new Dictionary<string, string> { ["X-CPF"] = "52998224725" },
            PathParameters = new Dictionary<string, string> { ["ID"] = OrdemServicoId.ToString() }
        };

        var response = await function.FunctionHandler(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.True);
            Assert.That(repository.CallCount, Is.EqualTo(1));
        });
    }

    private static HttpApiAuthorizerRequest CreateRequest(string? cpf, string? id)
    {
        return new HttpApiAuthorizerRequest
        {
            Headers = cpf is null
                ? null
                : new Dictionary<string, string> { ["x-cpf"] = cpf },
            PathParameters = id is null
                ? null
                : new Dictionary<string, string> { ["id"] = id }
        };
    }

    private sealed class FakeOrdemServicoAccessRepository(bool hasAccess) : IOrdemServicoAccessRepository
    {
        public int CallCount { get; private set; }
        public Guid? LastOrdemServicoId { get; private set; }
        public Cpf? LastCpf { get; private set; }

        public Task<bool> HasAccessAsync(
            Guid ordemServicoId,
            Cpf cpf,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastOrdemServicoId = ordemServicoId;
            LastCpf = cpf;
            return Task.FromResult(hasAccess);
        }
    }
}
