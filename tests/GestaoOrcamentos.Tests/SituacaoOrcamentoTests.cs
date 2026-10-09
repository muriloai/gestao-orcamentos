using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class SituacaoOrcamentoTests
{
    [Fact]
    public async Task TransicoesPreservamDadosEImpedemOperacoesProibidas()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            int clienteId;
            int aprovadoId;
            int recusadoId;
            int rascunhoId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                clienteId = await new ClienteService(escrita).CadastrarAsync(new ClienteFormulario
                {
                    Nome = "Cliente original",
                    Telefone = "1111",
                    Cidade = "Cidade antiga",
                    Observacoes = "Contato antigo"
                }, CancellationToken.None);
                escrita.ConfiguracoesNegocio.Add(new ConfiguracaoNegocio
                {
                    Id = 1,
                    Nome = "Negócio original",
                    Email = "original@exemplo.com",
                    Cidade = "Origem"
                });
                await escrita.SaveChangesAsync();

                var service = new OrcamentoService(escrita);
                aprovadoId = await service.CadastrarAsync(Formulario(clienteId, "Aprovar"), CancellationToken.None);
                recusadoId = await service.CadastrarAsync(Formulario(clienteId, "Recusar"), CancellationToken.None);
                rascunhoId = await service.CadastrarAsync(Formulario(clienteId, "Devolver"), CancellationToken.None);
                Assert.False(await service.AlterarSituacaoAsync(-1, SituacaoOrcamento.Pendente, CancellationToken.None));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Aprovado, CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Pendente, CancellationToken.None));
            }

            await using (var alteracao = new GestaoOrcamentosDbContext(options))
            {
                await new ClienteService(alteracao).AtualizarAsync(clienteId, new ClienteFormulario
                {
                    Nome = "Cliente atual",
                    Telefone = "2222",
                    Cidade = "Cidade nova"
                }, CancellationToken.None);
                var negocio = await alteracao.ConfiguracoesNegocio.SingleAsync();
                negocio.Nome = "Negócio atual";
                negocio.Email = "atual@exemplo.com";
                await alteracao.SaveChangesAsync();
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                var aprovado = (await service.ObterAsync(aprovadoId, CancellationToken.None))!;
                Assert.Equal(SituacaoOrcamento.Pendente, aprovado.Situacao);
                Assert.Equal("Cliente original", aprovado.ClienteRegistrado!.Nome);
                Assert.Equal("1111", aprovado.ClienteRegistrado.Telefone);
                Assert.Equal("Cidade antiga", aprovado.ClienteRegistrado.Cidade);
                Assert.Equal("Contato antigo", aprovado.ClienteRegistrado.Observacoes);
                Assert.Equal("Negócio original", aprovado.NegocioRegistrado!.Nome);
                Assert.Equal("original@exemplo.com", aprovado.NegocioRegistrado.Email);
                Assert.Equal("Origem", aprovado.NegocioRegistrado.Cidade);
                Assert.Equal("Cliente atual", aprovado.Cliente.Nome);
                Assert.Equal("Cliente original", aprovado.NomeClienteApresentado);

                var antigos = await service.ListarAsync("Cliente original", null, CancellationToken.None,
                    SituacaoOrcamento.Pendente);
                Assert.Single(antigos);
                Assert.Equal(aprovadoId, antigos[0].Id);
                Assert.Empty(await service.ListarAsync("Cliente atual", null, CancellationToken.None,
                    SituacaoOrcamento.Pendente));
                Assert.Equal(2, (await service.ListarAsync(null, null, CancellationToken.None,
                    SituacaoOrcamento.Rascunho)).Count);
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    service.ListarAsync(null, null, CancellationToken.None, (SituacaoOrcamento)99));
            }

            await using (var transicoes = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(transicoes);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.AtualizarAsync(aprovadoId, Formulario(clienteId, "Manipulado"), CancellationToken.None));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.ExcluirAsync(aprovadoId, CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Aprovado, CancellationToken.None));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.AlterarSituacaoAsync(aprovadoId, SituacaoOrcamento.Recusado, CancellationToken.None));

                Assert.True(await service.AlterarSituacaoAsync(recusadoId, SituacaoOrcamento.Pendente, CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(recusadoId, SituacaoOrcamento.Recusado, CancellationToken.None));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.AlterarSituacaoAsync(recusadoId, SituacaoOrcamento.Rascunho, CancellationToken.None));

                Assert.True(await service.AlterarSituacaoAsync(rascunhoId, SituacaoOrcamento.Pendente, CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(rascunhoId, SituacaoOrcamento.Rascunho, CancellationToken.None));
                var devolvido = (await service.ObterAsync(rascunhoId, CancellationToken.None))!;
                Assert.Null(devolvido.ClienteRegistrado);
                Assert.Null(devolvido.NegocioRegistrado);
                Assert.Equal("Cliente atual", devolvido.NomeClienteApresentado);
                Assert.True(await service.AtualizarAsync(rascunhoId,
                    OrcamentoFormulario.DoOrcamento(devolvido) with { Titulo = "Editado" }, CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(rascunhoId, SituacaoOrcamento.Pendente, CancellationToken.None));
            }

            await using (var final = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(final);
                var aprovado = (await service.ObterAsync(aprovadoId, CancellationToken.None))!;
                Assert.Equal(SituacaoOrcamento.Aprovado, aprovado.Situacao);
                Assert.Equal("Cliente original", aprovado.ClienteRegistrado!.Nome);
                Assert.Equal("Negócio original", aprovado.NegocioRegistrado!.Nome);
                var recusado = (await service.ObterAsync(recusadoId, CancellationToken.None))!;
                Assert.Equal(SituacaoOrcamento.Recusado, recusado.Situacao);
                var recapturado = (await service.ObterAsync(rascunhoId, CancellationToken.None))!;
                Assert.Equal("Editado", recapturado.Titulo);
                Assert.Equal("Cliente atual", recapturado.ClienteRegistrado!.Nome);
                Assert.Equal("Negócio atual", recapturado.NegocioRegistrado!.Nome);
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task PendenteExigeConfiguracaoESqlRejeitaSituacaoSemDados()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            int id;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var clienteId = await new ClienteService(escrita).CadastrarAsync(
                    new ClienteFormulario { Nome = "Cliente" }, CancellationToken.None);
                var service = new OrcamentoService(escrita);
                id = await service.CadastrarAsync(Formulario(clienteId, "Sem configuração"), CancellationToken.None);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.AlterarSituacaoAsync(id, SituacaoOrcamento.Pendente, CancellationToken.None));
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var orcamento = (await new OrcamentoService(leitura).ObterAsync(id, CancellationToken.None))!;
                Assert.Equal(SituacaoOrcamento.Rascunho, orcamento.Situacao);
                Assert.Null(orcamento.ClienteRegistrado);
                Assert.Null(orcamento.NegocioRegistrado);
                await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(() =>
                    leitura.Database.ExecuteSqlInterpolatedAsync($"UPDATE Orcamentos SET Situacao = 1 WHERE Id = {id}"));
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario Formulario(int clienteId, string titulo) => new()
    {
        ClienteId = clienteId,
        Titulo = titulo,
        DataEmissao = new DateOnly(2026, 10, 9),
        Itens = [new() { Descricao = "Serviço", Unidade = "un", Quantidade = 1, PrecoUnitario = 10 }]
    };
}