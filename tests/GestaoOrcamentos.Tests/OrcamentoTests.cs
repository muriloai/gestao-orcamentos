using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class OrcamentoTests
{
    [Fact]
    public async Task CadastroExigeClienteTituloDataEItensValidos()
    {
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>().Options;
        await using var context = new GestaoOrcamentosDbContext(options);
        var service = new OrcamentoService(context);

        foreach (var formulario in new[]
        {
            NovoFormulario() with { ClienteId = 0 },
            NovoFormulario() with { Titulo = " " },
            NovoFormulario() with { DataEmissao = null },
            NovoFormulario() with { Itens = [] },
            NovoFormulario() with { Itens = [new() { Descricao = " ", Unidade = "un", Quantidade = 1 }] },
            NovoFormulario() with { Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 0 }] },
            NovoFormulario() with { Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 1, PrecoUnitario = -1 }] },
            NovoFormulario() with { Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 1.0001m }] },
            NovoFormulario() with { Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 1, PrecoUnitario = 1.001m }] }
        })
        {
            await Assert.ThrowsAsync<ValidationException>(() => service.CadastrarAsync(formulario, CancellationToken.None));
        }
    }

    [Fact]
    public async Task CadastroCalculaArredondaPersisteEImpedeExcluirClienteVinculado()
    {
        var databaseName = $"GestaoOrcamentos_Teste_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var setup = new GestaoOrcamentosDbContext(options);
        try
        {
            await setup.Database.MigrateAsync();

            int clienteId;
            int primeiroId;
            int segundoId;
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                clienteId = await new ClienteService(escrita).CadastrarAsync(
                    new ClienteFormulario { Nome = "Cliente do orçamento" }, CancellationToken.None);

                var service = new OrcamentoService(escrita);
                var formulario = NovoFormulario() with
                {
                    ClienteId = clienteId,
                    Itens =
                    [
                        new() { Descricao = "Primeiro", Unidade = "un", Quantidade = 0.005m, PrecoUnitario = 1 },
                        new() { Descricao = "Segundo", Unidade = "hora", Quantidade = 0.005m, PrecoUnitario = 1 }
                    ]
                };
                primeiroId = await service.CadastrarAsync(formulario, CancellationToken.None);
                segundoId = await service.CadastrarAsync(NovoFormulario() with { ClienteId = clienteId },
                    CancellationToken.None);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.CadastrarAsync(NovoFormulario() with { ClienteId = int.MaxValue }, CancellationToken.None));
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    new ClienteService(escrita).ExcluirAsync(clienteId, CancellationToken.None));
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new OrcamentoService(leitura);
                var orcamento = await service.ObterAsync(primeiroId, CancellationToken.None);

                Assert.NotNull(orcamento);
                Assert.Equal("000001", orcamento.Numero);
                Assert.Equal("Cliente do orçamento", orcamento.Cliente.Nome);
                Assert.Equal(2, orcamento.Itens.Count);
                Assert.All(orcamento.Itens, item => Assert.Equal(0.01m, item.Subtotal));
                Assert.Equal(0.02m, orcamento.Total);
                Assert.NotEqual(primeiroId, segundoId);
                Assert.Null(await service.ObterAsync(-1, CancellationToken.None));

                var cliente = await leitura.Clientes.SingleAsync(item => item.Id == clienteId);
                leitura.Clientes.Remove(cliente);
                await Assert.ThrowsAsync<DbUpdateException>(() => leitura.SaveChangesAsync());
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }

    private static OrcamentoFormulario NovoFormulario() => new()
    {
        ClienteId = 1,
        Titulo = "  Proposta  ",
        DataEmissao = new DateOnly(2026, 10, 8),
        Itens = [new() { Descricao = "Item", Unidade = "un", Quantidade = 1, PrecoUnitario = 10 }]
    };
}