using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class ListagemOrcamentosTests
{
    [Fact]
    public async Task ListaPesquisaEFiltraOrcamentosComTotais()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();

            int aliceId;
            int bobId;
            int semOrcamentosId;
            int primeiroId;
            int segundoId;
            int terceiroId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var clientes = new ClienteService(escrita);
                aliceId = await clientes.CadastrarAsync(new ClienteFormulario { Nome = "Alice" }, CancellationToken.None);
                bobId = await clientes.CadastrarAsync(new ClienteFormulario { Nome = "Bob" }, CancellationToken.None);
                semOrcamentosId = await clientes.CadastrarAsync(new ClienteFormulario { Nome = "Carla" }, CancellationToken.None);

                var orcamentos = new OrcamentoService(escrita);
                primeiroId = await orcamentos.CadastrarAsync(Formulario(aliceId, "Projeto Alpha", 8, 10), CancellationToken.None);
                segundoId = await orcamentos.CadastrarAsync(Formulario(bobId, "Projeto Beta", 9, 20), CancellationToken.None);
                terceiroId = await orcamentos.CadastrarAsync(Formulario(aliceId, "Projeto Beta", 10, 0.01m), CancellationToken.None);
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                var todos = await service.ListarAsync(null, null, CancellationToken.None);
                Assert.Equal([terceiroId, segundoId, primeiroId], todos.Select(item => item.Id));
                Assert.Equal([0.01m, 20m, 10m], todos.Select(item => item.Total));
                Assert.Equal(["Alice", "Bob", "Alice"], todos.Select(item => item.Cliente.Nome));

                var porCliente = await service.ListarAsync("  Alice  ", null, CancellationToken.None);
                Assert.Equal([terceiroId, primeiroId], porCliente.Select(item => item.Id));

                var porTitulo = await service.ListarAsync("  Beta  ", null, CancellationToken.None);
                Assert.Equal([terceiroId, segundoId], porTitulo.Select(item => item.Id));

                var daAlice = await service.ListarAsync(null, aliceId, CancellationToken.None);
                Assert.Equal([terceiroId, primeiroId], daAlice.Select(item => item.Id));
                Assert.Single(await service.ListarAsync(null, bobId, CancellationToken.None));
                Assert.Empty(await service.ListarAsync(null, semOrcamentosId, CancellationToken.None));
                Assert.Empty(await service.ListarAsync("inexistente", null, CancellationToken.None));
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario Formulario(int clienteId, string titulo, int dia, decimal preco) => new()
    {
        ClienteId = clienteId,
        Titulo = titulo,
        DataEmissao = new DateOnly(2026, 10, dia),
        Itens = [new() { Descricao = "Serviço", Unidade = "un", Quantidade = 1, PrecoUnitario = preco }]
    };
}