using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class ClienteTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeVazioNaoEValido(string nome)
    {
        var cadastro = new ClienteFormulario { Nome = nome };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(cadastro, new ValidationContext(cadastro), resultados, true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.MemberNames.Contains(nameof(ClienteFormulario.Nome)));
    }

    [Fact]
    public void EmailInvalidoNaoEValido()
    {
        var cadastro = new ClienteFormulario { Nome = "Cliente", Email = "invalido" };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(cadastro, new ValidationContext(cadastro), resultados, true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.MemberNames.Contains(nameof(ClienteFormulario.Email)));
    }

    [Fact]
    public async Task ServicoRejeitaCadastroInvalidoAntesDeGravar()
    {
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>().Options;
        await using var context = new GestaoOrcamentosDbContext(options);
        var service = new ClienteService(context);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CadastrarAsync(new ClienteFormulario { Nome = " " }, CancellationToken.None));
    }

    [Fact]
    public async Task CadastroConsultaEEdicaoPersistemEntreContextos()
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
            await using (var escrita = new GestaoOrcamentosDbContext(options))
            {
                var service = new ClienteService(escrita);
                clienteId = await service.CadastrarAsync(new ClienteFormulario { Nome = "  Cliente de teste  " },
                    CancellationToken.None);
                await service.CadastrarAsync(new ClienteFormulario { Nome = "Outra pessoa" }, CancellationToken.None);
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var service = new ClienteService(leitura);
                var todos = await service.ListarAsync(null, CancellationToken.None);
                var encontrados = await service.ListarAsync("  Cliente de teste  ", CancellationToken.None);
                var semResultados = await service.ListarAsync("inexistente", CancellationToken.None);
                var cliente = await service.ObterAsync(clienteId, CancellationToken.None);

                Assert.Equal(["Cliente de teste", "Outra pessoa"], todos.Select(item => item.Nome));
                Assert.Single(encontrados);
                Assert.Equal(clienteId, encontrados[0].Id);
                Assert.Empty(semResultados);
                Assert.NotNull(cliente);
                Assert.Equal("Cliente de teste", cliente.Nome);
                Assert.Null(cliente.Email);
                Assert.Null(cliente.Cidade);
                Assert.Null(await service.ObterAsync(-1, CancellationToken.None));

                Assert.True(await service.AtualizarAsync(clienteId, new ClienteFormulario
                {
                    Nome = "Cliente atualizado",
                    Email = "cliente@exemplo.com",
                    Cidade = "São Paulo"
                }, CancellationToken.None));
                Assert.False(await service.AtualizarAsync(-1, new ClienteFormulario { Nome = "Ninguém" },
                    CancellationToken.None));
                await Assert.ThrowsAsync<ValidationException>(() => service.AtualizarAsync(clienteId,
                    new ClienteFormulario { Nome = " ", Email = "invalido" }, CancellationToken.None));
            }

            await using (var confirmacao = new GestaoOrcamentosDbContext(options))
            {
                var cliente = await confirmacao.Clientes.SingleAsync(item => item.Id == clienteId,
                    CancellationToken.None);

                Assert.Equal("Cliente atualizado", cliente.Nome);
                Assert.Equal("cliente@exemplo.com", cliente.Email);
                Assert.Equal("São Paulo", cliente.Cidade);
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }
}
