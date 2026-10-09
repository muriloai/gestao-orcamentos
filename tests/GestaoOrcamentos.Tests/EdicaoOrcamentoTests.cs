using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class EdicaoOrcamentoTests
{
    [Fact]
    public async Task EditarItensRecalculaEExcluirRemoveRascunho()
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
            int orcamentoId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                clienteId = await new ClienteService(escrita).CadastrarAsync(
                    new ClienteFormulario { Nome = "Cliente" }, CancellationToken.None);
                orcamentoId = await new OrcamentoService(escrita).CadastrarAsync(new OrcamentoFormulario
                {
                    ClienteId = clienteId,
                    Titulo = "Original",
                    DataEmissao = new DateOnly(2026, 10, 8),
                    Itens =
                    [
                        new() { Descricao = "Manter", Unidade = "un", Quantidade = 1, PrecoUnitario = 10 },
                        new() { Descricao = "Remover", Unidade = "un", Quantidade = 1, PrecoUnitario = 20 }
                    ]
                }, CancellationToken.None);
            }

            int idMantido;
            await using (var edicao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(edicao);
                var formulario = OrcamentoFormulario.DoOrcamento((await service.ObterAsync(orcamentoId, CancellationToken.None))!);
                idMantido = formulario.Itens[0].Id;
                formulario.Titulo = "  Editado  ";
                formulario.Itens[0].Quantidade = 2;
                formulario.Itens.RemoveAt(1);
                formulario.Itens.Add(new ItemOrcamentoFormulario
                {
                    Descricao = "Novo",
                    Unidade = "hora",
                    Quantidade = 0.005m,
                    PrecoUnitario = 1
                });

                Assert.True(await service.AtualizarAsync(orcamentoId, formulario, CancellationToken.None));
                Assert.False(await service.AtualizarAsync(-1, formulario, CancellationToken.None));
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                var orcamento = (await service.ObterAsync(orcamentoId, CancellationToken.None))!;
                Assert.Equal("Editado", orcamento.Titulo);
                Assert.Equal(2, orcamento.Itens.Count);
                Assert.Contains(orcamento.Itens, item => item.Id == idMantido && item.Subtotal == 20m);
                Assert.Contains(orcamento.Itens, item => item.Descricao == "Novo" && item.Subtotal == 0.01m);
                Assert.Equal(20.01m, orcamento.Total);

                var semItens = OrcamentoFormulario.DoOrcamento(orcamento) with { Titulo = "Não gravar", Itens = [] };
                await Assert.ThrowsAsync<ValidationException>(() =>
                    service.AtualizarAsync(orcamentoId, semItens, CancellationToken.None));

                var itemEstranho = OrcamentoFormulario.DoOrcamento(orcamento);
                itemEstranho.Itens[0].Id = int.MaxValue;
                await Assert.ThrowsAsync<ValidationException>(() =>
                    service.AtualizarAsync(orcamentoId, itemEstranho, CancellationToken.None));
            }

            await using (var confirmacao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(confirmacao);
                Assert.Equal("Editado", (await service.ObterAsync(orcamentoId, CancellationToken.None))!.Titulo);
                Assert.Equal(clienteId, await service.ExcluirAsync(orcamentoId, CancellationToken.None));
                Assert.Null(await service.ExcluirAsync(orcamentoId, CancellationToken.None));
                Assert.Empty(await confirmacao.ItensOrcamento.ToListAsync());
                Assert.True(await new ClienteService(confirmacao).ExcluirAsync(clienteId, CancellationToken.None));
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task FalhaNoBancoDuranteEdicaoNaoGravaCabecalhoNemItens()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();
            int orcamentoId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var clienteId = await new ClienteService(escrita).CadastrarAsync(
                    new ClienteFormulario { Nome = "Cliente" }, CancellationToken.None);
                orcamentoId = await new OrcamentoService(escrita).CadastrarAsync(new OrcamentoFormulario
                {
                    ClienteId = clienteId,
                    Titulo = "Antes",
                    DataEmissao = new DateOnly(2026, 10, 8),
                    Itens = [new() { Descricao = "Original", Unidade = "un", Quantidade = 1, PrecoUnitario = 5 }]
                }, CancellationToken.None);
            }

            await setup.Database.ExecuteSqlRawAsync("""
                CREATE TRIGGER trg_TesteBloquearItem ON ItensOrcamento AFTER INSERT AS
                BEGIN
                    THROW 50001, 'Falha simulada ao gravar item.', 1;
                END
                """);

            await using (var edicao = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(edicao);
                var formulario = OrcamentoFormulario.DoOrcamento((await service.ObterAsync(orcamentoId, CancellationToken.None))!);
                formulario.Titulo = "Depois";
                formulario.Itens.Add(new ItemOrcamentoFormulario
                {
                    Descricao = "Novo",
                    Unidade = "un",
                    Quantidade = 1,
                    PrecoUnitario = 10
                });

                await Assert.ThrowsAsync<DbUpdateException>(() =>
                    service.AtualizarAsync(orcamentoId, formulario, CancellationToken.None));
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var orcamento = (await new OrcamentoService(leitura).ObterAsync(orcamentoId, CancellationToken.None))!;
                Assert.Equal("Antes", orcamento.Titulo);
                Assert.Single(orcamento.Itens);
                Assert.Equal(5m, orcamento.Total);
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }
}