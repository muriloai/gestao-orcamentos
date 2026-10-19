using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class AlertasOrcamentoTests
{
    [Fact]
    public async Task RegistraUmaVezSomenteOrcamentosPendentesVencidosNaDataDeSaoPaulo()
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

            var clienteId = await new ClienteService(setup).CadastrarAsync(
                new ClienteFormulario { Nome = "Cliente dos avisos" }, CancellationToken.None);
            var orcamentos = new OrcamentoService(setup);
            var vencidoPendente = await orcamentos.CadastrarAsync(
                Formulario(clienteId, new DateOnly(2026, 10, 14)), CancellationToken.None);
            var validoHoje = await orcamentos.CadastrarAsync(
                Formulario(clienteId, new DateOnly(2026, 10, 15)), CancellationToken.None);
            var vencidoRascunho = await orcamentos.CadastrarAsync(
                Formulario(clienteId, new DateOnly(2026, 10, 14)), CancellationToken.None);

            await orcamentos.AlterarSituacaoAsync(vencidoPendente, SituacaoOrcamento.Pendente, CancellationToken.None);
            await orcamentos.AlterarSituacaoAsync(validoHoje, SituacaoOrcamento.Pendente, CancellationToken.None);

            var relogio = new RelogioFalso(new DateTimeOffset(2026, 10, 16, 1, 0, 0, TimeSpan.Zero));
            var alertas = new AlertasOrcamentoService(setup, relogio);

            Assert.Equal(new DateOnly(2026, 10, 15), alertas.DataReferencia);
            Assert.Equal(1, await alertas.RegistrarVencidosAsync(CancellationToken.None));
            Assert.Equal(0, await alertas.RegistrarVencidosAsync(CancellationToken.None));

            var vencidos = await alertas.ListarVencidosAsync(CancellationToken.None);
            var aviso = Assert.Single(vencidos);
            Assert.Equal(vencidoPendente, aviso.Id);
            Assert.NotEqual(vencidoRascunho, aviso.Id);

            await orcamentos.AlterarSituacaoAsync(vencidoPendente, SituacaoOrcamento.Aprovado, CancellationToken.None);
            Assert.Empty(await alertas.ListarVencidosAsync(CancellationToken.None));
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario Formulario(int clienteId, DateOnly validade) => new()
    {
        ClienteId = clienteId,
        Titulo = "Proposta",
        DataEmissao = new DateOnly(2026, 10, 1),
        Validade = validade,
        Itens = [new() { Descricao = "Serviço", Unidade = "un", Quantidade = 1, PrecoUnitario = 50m }]
    };

    private sealed class RelogioFalso(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
