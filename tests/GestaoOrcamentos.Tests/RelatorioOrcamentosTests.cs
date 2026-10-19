using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class RelatorioOrcamentosTests
{
    [Fact]
    public async Task RelatorioContaSituacoesEIncluiAsDatasDeLimite()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            setup.ConfiguracoesNegocio.Add(new ConfiguracaoNegocio { Id = 1, Nome = "Negócio de teste" });
            await setup.SaveChangesAsync();

            var clientes = new ClienteService(setup);
            var clienteId = await clientes.CadastrarAsync(new ClienteFormulario { Nome = "Cliente do relatório" }, CancellationToken.None);
            var orcamentos = new OrcamentoService(setup);
            var rascunhoId = await orcamentos.CadastrarAsync(Formulario(clienteId, new DateOnly(2026, 10, 10), 10), CancellationToken.None);
            var pendenteId = await orcamentos.CadastrarAsync(Formulario(clienteId, new DateOnly(2026, 10, 11), 20), CancellationToken.None);
            var aprovadoId = await orcamentos.CadastrarAsync(Formulario(clienteId, new DateOnly(2026, 10, 12), 30), CancellationToken.None);
            var recusadoId = await orcamentos.CadastrarAsync(Formulario(clienteId, new DateOnly(2026, 10, 9), 40), CancellationToken.None);

            await orcamentos.AlterarSituacaoAsync(pendenteId, SituacaoOrcamento.Pendente, CancellationToken.None);
            await orcamentos.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Pendente, CancellationToken.None);
            await orcamentos.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Aprovado, CancellationToken.None);
            await orcamentos.AlterarSituacaoAsync(recusadoId, SituacaoOrcamento.Pendente, CancellationToken.None);
            await orcamentos.AlterarSituacaoAsync(recusadoId, SituacaoOrcamento.Recusado, CancellationToken.None);

            var relatorio = new RelatorioOrcamentosService(setup);
            var situacoes = await relatorio.ConsultarPeriodoAsync(
                new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12), CancellationToken.None);

            Assert.Equal(4, situacoes.Count);
            Assert.Equal((1, 10m), (situacoes[0].QuantidadeOrcamentos, situacoes[0].ValorTotal));
            Assert.Equal((1, 20m), (situacoes[1].QuantidadeOrcamentos, situacoes[1].ValorTotal));
            Assert.Equal("Propostas aprovadas", situacoes[2].Descricao);
            Assert.Equal((1, 30m), (situacoes[2].QuantidadeOrcamentos, situacoes[2].ValorTotal));
            Assert.Equal((0, 0m), (situacoes[3].QuantidadeOrcamentos, situacoes[3].ValorTotal));

            var resumo = await orcamentos.ListarResumosAsync("Cliente do relatório", null, CancellationToken.None);
            Assert.Contains(resumo, item => item.Id == rascunhoId && item.NomeCliente == "Cliente do relatório");
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario Formulario(int clienteId, DateOnly emissao, decimal preco) => new()
    {
        ClienteId = clienteId,
        Titulo = "Serviço",
        DataEmissao = emissao,
        Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 1, PrecoUnitario = preco }]
    };
}
