using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Persistence.Entities;
using FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.Tests;

public sealed class OrdemServicoAccessRepositoryTests
{
    private static readonly Guid ClienteId = Guid.Parse("80b8789a-4348-4cf8-aac5-4bd30a96f01e");
    private static readonly Guid OrdemServicoId = Guid.Parse("836f2e61-9d48-43e5-abef-c981cc68d435");
    private static readonly Guid CodigoAprovacao = Guid.Parse("79ee2120-ab05-4cba-a57d-011d654248dd");
    private const string CpfValue = "52998224725";

    [Test]
    public async Task HasAccessAsync_ShouldReturnTrue_WhenTokenMatchesActiveOrdemServico()
    {
        var fixture = await CreateFixtureAsync();
        var cpf = CreateCpf(CpfValue);

        var result = await fixture.Repository.HasAccessAsync(OrdemServicoId, CpfAccessToken.Create(cpf, CodigoAprovacao));

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenTokenDoesNotMatchOrdemServico()
    {
        var fixture = await CreateFixtureAsync();
        var cpf = CreateCpf("11144477735");

        var result = await fixture.Repository.HasAccessAsync(OrdemServicoId, CpfAccessToken.Create(cpf, CodigoAprovacao));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenOrdemServicoIsInactive()
    {
        var fixture = await CreateFixtureAsync(ordemServicoAtiva: false);
        var cpf = CreateCpf(CpfValue);

        var result = await fixture.Repository.HasAccessAsync(OrdemServicoId, CpfAccessToken.Create(cpf, CodigoAprovacao));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasAccessAsync_ShouldReturnFalse_WhenClienteIsInactive()
    {
        var fixture = await CreateFixtureAsync(clienteAtivo: false);
        var cpf = CreateCpf(CpfValue);

        var result = await fixture.Repository.HasAccessAsync(OrdemServicoId, CpfAccessToken.Create(cpf, CodigoAprovacao));

        Assert.That(result, Is.False);
    }

    private static async Task<RepositoryFixture> CreateFixtureAsync(
        bool clienteAtivo = true,
        bool ordemServicoAtiva = true)
    {
        var databaseName = Guid.NewGuid().ToString();
        var databaseRoot = new InMemoryDatabaseRoot();

        AuthorizationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AuthorizationDbContext>()
                .UseInMemoryDatabase(databaseName, databaseRoot)
                .Options;

            return new AuthorizationDbContext(options);
        }

        await using (var context = CreateContext())
        {
            var cliente = new ClienteAccessEntity
            {
                Id = ClienteId,
                Ativo = clienteAtivo,
                Cpf = CpfValue
            };

            var ordemServico = new OrdemServicoAccessEntity
            {
                Id = OrdemServicoId,
                CodigoAprovacao = CodigoAprovacao,
                Ativo = ordemServicoAtiva,
                ClienteId = ClienteId,
                Cliente = cliente
            };

            _ = context.Clientes.Add(cliente);
            _ = context.OrdensServico.Add(ordemServico);
            _ = await context.SaveChangesAsync();
        }

        var repository = new OrdemServicoAccessRepository(
            _ => Task.FromResult(CreateContext()));

        return new RepositoryFixture(repository);
    }

    private static Cpf CreateCpf(string value)
    {
        if (!Cpf.TryCreate(value, out var cpf))
            throw new InvalidOperationException("O CPF de teste deve ser valido.");

        return cpf;
    }

    private sealed record RepositoryFixture(OrdemServicoAccessRepository Repository);
}
