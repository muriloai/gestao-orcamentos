using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class DuplicacaoOrcamentoTests
{
    [Fact]
    public async Task DuplicaCadaSituacaoComoRascunhoIndependente()
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
            int[] originais;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                clienteId = await new ClienteService(escrita).CadastrarAsync(new ClienteFormulario
                {
                    Nome = "Cliente anterior"
                }, CancellationToken.None);
                escrita.ConfiguracoesNegocio.Add(new ConfiguracaoNegocio { Id = 1, Nome = "Negócio anterior" });
                await escrita.SaveChangesAsync();

                var service = new OrcamentoService(escrita);
                originais = [];
                foreach (var titulo in new[] { "Rascunho", "Pendente", "Aprovado", "Recusado" })
                {
                    var id = await service.CadastrarAsync(new OrcamentoFormulario
                    {
                        ClienteId = clienteId,
                        Titulo = titulo,
                        DataEmissao = new DateOnly(2026, 1, 1),
                        Validade = new DateOnly(2026, 12, 31),
                        Observacoes = "Observação copiada",
                        Itens =
                        [
                            new() { Descricao = "Fotografia", Quantidade = 2, Unidade = "hora", PrecoUnitario = 25 },
                            new() { Descricao = "Edição", Quantidade = 0.5m, Unidade = "hora", PrecoUnitario = 30 }
                        ]
                    }, CancellationToken.None);
                    originais = [.. originais, id];
                }

                foreach (var id in originais.Skip(1))
                {
                    Assert.True(await service.AlterarSituacaoAsync(id, SituacaoOrcamento.Pendente, CancellationToken.None));
                }

                Assert.True(await service.AlterarSituacaoAsync(originais[2], SituacaoOrcamento.Aprovado,
                    CancellationToken.None));
                Assert.True(await service.AlterarSituacaoAsync(originais[3], SituacaoOrcamento.Recusado,
                    CancellationToken.None));
            }

            await using (var alteracao = new GestaoOrcamentosDbContext(options))
            {
                Assert.True(await new ClienteService(alteracao).AtualizarAsync(clienteId,
                    new ClienteFormulario { Nome = "Cliente atual" }, CancellationToken.None));
                var negocio = await alteracao.ConfiguracoesNegocio.SingleAsync();
                negocio.Nome = "Negócio atual";
                await alteracao.SaveChangesAsync();
            }

            var antes = HojeEmSaoPaulo();
            int[] copias;
            await using (var duplicacao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(duplicacao);
                copias = [];
                foreach (var id in originais)
                {
                    var copiaId = await service.DuplicarAsync(id, CancellationToken.None);
                    Assert.NotNull(copiaId);
                    copias = [.. copias, copiaId.Value];
                }

                Assert.Null(await service.DuplicarAsync(-1, CancellationToken.None));
            }
            var depois = HojeEmSaoPaulo();

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                Assert.Equal(8, (await service.ListarAsync(null, clienteId, CancellationToken.None)).Count);
                Assert.Equal(8, originais.Concat(copias).Distinct().Count());

                for (var i = 0; i < originais.Length; i++)
                {
                    var original = (await service.ObterAsync(originais[i], CancellationToken.None))!;
                    var copia = (await service.ObterAsync(copias[i], CancellationToken.None))!;

                    Assert.Equal(SituacaoOrcamento.Rascunho, copia.Situacao);
                    Assert.Equal(clienteId, copia.ClienteId);
                    Assert.Equal("Cliente atual", copia.NomeClienteApresentado);
                    Assert.Equal(original.Titulo, copia.Titulo);
                    Assert.Equal(original.Observacoes, copia.Observacoes);
                    Assert.Equal(original.Total, copia.Total);
                    Assert.Null(copia.Validade);
                    Assert.InRange(copia.DataEmissao, antes, depois);
                    Assert.Null(copia.ClienteRegistrado);
                    Assert.Null(copia.NegocioRegistrado);
                    Assert.NotEqual(original.Numero, copia.Numero);
                    Assert.Equal(original.Itens.Count, copia.Itens.Count);
                    Assert.Empty(original.Itens.Select(item => item.Id).Intersect(copia.Itens.Select(item => item.Id)));
                    Assert.Equal(original.Itens.OrderBy(item => item.Id).Select(item =>
                            (item.Descricao, item.Quantidade, item.Unidade, item.PrecoUnitario)),
                        copia.Itens.OrderBy(item => item.Id).Select(item =>
                            (item.Descricao, item.Quantidade, item.Unidade, item.PrecoUnitario)));
                }

                var aprovado = (await service.ObterAsync(originais[2], CancellationToken.None))!;
                Assert.Equal("Cliente anterior", aprovado.ClienteRegistrado!.Nome);
                Assert.Equal("Negócio anterior", aprovado.NegocioRegistrado!.Nome);
            }

            await using (var edicao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(edicao);
                var copia = (await service.ObterAsync(copias[0], CancellationToken.None))!;
                var formulario = OrcamentoFormulario.DoOrcamento(copia);
                formulario.Itens[0].PrecoUnitario = 99;
                Assert.True(await service.AtualizarAsync(copias[0], formulario, CancellationToken.None));
            }

            await using (var confirmacao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(confirmacao);
                Assert.Equal(65m, (await service.ObterAsync(originais[0], CancellationToken.None))!.Total);
                Assert.Equal(213m, (await service.ObterAsync(copias[0], CancellationToken.None))!.Total);
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static DateOnly HojeEmSaoPaulo() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")));
}