using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class ClienteCadastroTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeVazioNaoEValido(string nome)
    {
        var cadastro = new ClienteCadastro { Nome = nome };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(cadastro, new ValidationContext(cadastro), resultados, true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.MemberNames.Contains(nameof(ClienteCadastro.Nome)));
    }

    [Fact]
    public void EmailInvalidoNaoEValido()
    {
        var cadastro = new ClienteCadastro { Nome = "Cliente", Email = "invalido" };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(cadastro, new ValidationContext(cadastro), resultados, true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.MemberNames.Contains(nameof(ClienteCadastro.Email)));
    }

    [Fact]
    public async Task ServicoRejeitaCadastroInvalidoAntesDeGravar()
    {
        var options = new DbContextOptionsBuilder<GestaoOrcamentosDbContext>().Options;
        await using var context = new GestaoOrcamentosDbContext(options);
        var service = new ClienteService(context);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CadastrarAsync(new ClienteCadastro { Nome = " " }, CancellationToken.None));
    }

    [Fact]
    public async Task CadastroComSomenteNomePersisteEntreContextos()
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
                clienteId = await service.CadastrarAsync(new ClienteCadastro { Nome = "  Cliente de teste  " },
                    CancellationToken.None);
            }

            await using (var leitura = new GestaoOrcamentosDbContext(options))
            {
                var cliente = await leitura.Clientes.SingleAsync(item => item.Id == clienteId,
                    CancellationToken.None);

                Assert.Equal("Cliente de teste", cliente.Nome);
                Assert.Null(cliente.Email);
                Assert.Null(cliente.Cidade);
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }
}