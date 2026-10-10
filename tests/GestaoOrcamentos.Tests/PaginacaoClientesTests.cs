using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class PaginacaoClientesTests
{
    [Fact]
    public async Task PesquisaPaginadaMantemOrdemEstavelEInformaTotais()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        await using var setup = new GestaoOrcamentosDbContext(options);

        try
        {
            await setup.Database.MigrateAsync();
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var service = new ClienteService(escrita);
                foreach (var nome in new[] { "Bia", "Ana", "Ana", "Ana", "Caio" })
                {
                    await service.CadastrarAsync(new ClienteFormulario { Nome = nome }, CancellationToken.None);
                }
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new ClienteService(leitura);
                var primeira = await service.ListarPaginadoAsync(null, 1, 2, CancellationToken.None);
                var segunda = await service.ListarPaginadoAsync(null, 2, 2, CancellationToken.None);
                var terceira = await service.ListarPaginadoAsync(null, 3, 2, CancellationToken.None);
                Assert.Equal(5, primeira.TotalItens);
                Assert.Equal(5, segunda.TotalItens);
                Assert.Equal(5, terceira.TotalItens);
                Assert.Equal(["Ana", "Ana"], primeira.Itens.Select(cliente => cliente.Nome));
                Assert.Equal(["Ana", "Bia"], segunda.Itens.Select(cliente => cliente.Nome));
                Assert.Equal(["Caio"], terceira.Itens.Select(cliente => cliente.Nome));
                Assert.Equal(5, new[] { primeira, segunda, terceira }
                    .SelectMany(pagina => pagina.Itens).Select(cliente => cliente.Id).Distinct().Count());

                var filtrada = await service.ListarPaginadoAsync(" Ana ", 2, 2, CancellationToken.None);
                Assert.Equal(3, filtrada.TotalItens);
                Assert.Single(filtrada.Itens);
                Assert.Equal("Ana", filtrada.Itens[0].Nome);
                Assert.Empty((await service.ListarPaginadoAsync("Inexistente", 1, 20, CancellationToken.None)).Itens);
                Assert.Empty((await service.ListarPaginadoAsync(null, int.MaxValue, 100, CancellationToken.None)).Itens);

                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    service.ListarPaginadoAsync(null, 0, 20, CancellationToken.None));
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    service.ListarPaginadoAsync(null, 1, 101, CancellationToken.None));
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }
}