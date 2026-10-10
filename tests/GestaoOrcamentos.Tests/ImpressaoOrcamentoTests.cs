using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class ImpressaoOrcamentoTests
{
    [Fact]
    public async Task ImpressaoUsaCadastrosAtuaisNoRascunhoEHistoricosAposEnvio()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            int rascunhoId;
            int enviadoId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var clienteId = await new ClienteService(escrita).CadastrarAsync(
                    new ClienteFormulario { Nome = "Cliente antigo", Cidade = "Cidade antiga" }, CancellationToken.None);
                escrita.ConfiguracoesNegocio.Add(new ConfiguracaoNegocio { Id = 1, Nome = "Negócio antigo" });
                await escrita.SaveChangesAsync();

                var service = new OrcamentoService(escrita);
                rascunhoId = await service.CadastrarAsync(Formulario(clienteId), CancellationToken.None);
                enviadoId = await service.CadastrarAsync(Formulario(clienteId), CancellationToken.None);
                await service.AlterarSituacaoAsync(enviadoId, SituacaoOrcamento.Pendente, CancellationToken.None);
            }

            await using (var alteracao = new GestaoOrcamentosDbContext(options))
            {
                var cliente = await alteracao.Clientes.SingleAsync();
                cliente.Nome = "Cliente atual";
                cliente.Cidade = "Cidade atual";
                var negocio = await alteracao.ConfiguracoesNegocio.SingleAsync();
                negocio.Nome = "Negócio atual";
                await alteracao.SaveChangesAsync();
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                var rascunho = (await service.ObterParaImpressaoAsync(rascunhoId, CancellationToken.None))!;
                var enviado = (await service.ObterParaImpressaoAsync(enviadoId, CancellationToken.None))!;
                Assert.Equal("Cliente atual", rascunho.Cliente.Nome);
                Assert.Equal("Cidade atual", rascunho.Cliente.Cidade);
                Assert.Equal("Negócio atual", rascunho.Negocio!.Nome);
                Assert.Equal("Cliente antigo", enviado.Cliente.Nome);
                Assert.Equal("Cidade antiga", enviado.Cliente.Cidade);
                Assert.Equal("Negócio antigo", enviado.Negocio!.Nome);
                Assert.Equal(20.01m, enviado.Orcamento.Total);
                Assert.Null(await service.ObterParaImpressaoAsync(-1, CancellationToken.None));
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario Formulario(int clienteId) => new()
    {
        ClienteId = clienteId,
        Titulo = "Proposta de serviço",
        DataEmissao = new DateOnly(2026, 10, 9),
        Validade = new DateOnly(2026, 10, 20),
        Observacoes = "Condições da proposta",
        Itens =
        [
            new() { Descricao = "Primeiro item", Quantidade = 1.001m, Unidade = "un", PrecoUnitario = 10.00m },
            new() { Descricao = "Segundo item", Quantidade = 1, Unidade = "un", PrecoUnitario = 10.00m }
        ]
    };
}