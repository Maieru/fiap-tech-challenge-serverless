using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Contracts;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class FunctionTests
{
    private static readonly Guid OrdemServicoId = Guid.Parse("9be8b471-bd51-4f44-a0e0-3db12cb342c3");
    private const string AccessToken = "b5e74f864f72daddbd995596a65e6ac368aa2c362e3858f7a38dc9c01eaba471";

    [Test]
    public async Task FunctionHandler_ShouldAllowRequest_WhenTokenAuthorizesOrdemServico()
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);
        var request = CreateRequest(AccessToken, OrdemServicoId.ToString());

        var response = await function.FunctionHandler(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.True);
            Assert.That(response.Context["ordemServicoId"], Is.EqualTo(OrdemServicoId.ToString()));
            Assert.That(repository.CallCount, Is.EqualTo(1));
            Assert.That(repository.LastOrdemServicoId, Is.EqualTo(OrdemServicoId));
            Assert.That(repository.LastAccessToken, Is.EqualTo(AccessToken));
        });
    }

    [Test]
    public async Task FunctionHandler_ShouldDenyRequest_WhenCpfDoesNotBelongToOrdemServico()
    {
        var repository = new FakeOrdemServicoAccessRepository(false);
        var function = new Function(repository);

        var response = await function.FunctionHandler(CreateRequest(AccessToken, OrdemServicoId.ToString()));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.False);
            Assert.That(response.Context, Is.Empty);
            Assert.That(repository.CallCount, Is.EqualTo(1));
        });
    }

    [TestCase(null)]
    [TestCase("token-invalido")]
    [TestCase("b5e74f864f72daddbd995596a65e6ac368aa2c362e3858f7a38dc9c01eaba47z")]
    public async Task FunctionHandler_ShouldDenyWithoutQueryingDatabase_WhenTokenIsMissingOrInvalid(string? token)
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);

        var response = await function.FunctionHandler(CreateRequest(token, OrdemServicoId.ToString()));

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

        var response = await function.FunctionHandler(CreateRequest(AccessToken, id));

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.False);
            Assert.That(repository.CallCount, Is.Zero);
        });
    }

    [Test]
    public async Task FunctionHandler_ShouldReadPathParameterNameCaseInsensitively()
    {
        var repository = new FakeOrdemServicoAccessRepository(true);
        var function = new Function(repository);
        var request = new HttpApiAuthorizerRequest
        {
            QueryStringParameters = new Dictionary<string, string> { ["token"] = AccessToken },
            PathParameters = new Dictionary<string, string> { ["ID"] = OrdemServicoId.ToString() }
        };

        var response = await function.FunctionHandler(request);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsAuthorized, Is.True);
            Assert.That(repository.CallCount, Is.EqualTo(1));
        });
    }

    private static HttpApiAuthorizerRequest CreateRequest(string? token, string? id)
    {
        return new HttpApiAuthorizerRequest
        {
            QueryStringParameters = token is null
                ? null
                : new Dictionary<string, string> { ["token"] = token },
            PathParameters = id is null
                ? null
                : new Dictionary<string, string> { ["id"] = id }
        };
    }

    private sealed class FakeOrdemServicoAccessRepository(bool hasAccess) : IOrdemServicoAccessRepository
    {
        public int CallCount { get; private set; }
        public Guid? LastOrdemServicoId { get; private set; }
        public string? LastAccessToken { get; private set; }

        public Task<bool> HasAccessAsync(
            Guid ordemServicoId,
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastOrdemServicoId = ordemServicoId;
            LastAccessToken = accessToken;
            return Task.FromResult(hasAccess);
        }
    }
}
