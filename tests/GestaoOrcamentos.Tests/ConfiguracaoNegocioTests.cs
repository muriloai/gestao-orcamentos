using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Tests;

public class ConfiguracaoNegocioTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeVazioNaoEValido(string nome)
    {
        var formulario = new ConfiguracaoFormulario { Nome = nome };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(formulario, new ValidationContext(formulario), resultados, true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.MemberNames.Contains(nameof(ConfiguracaoFormulario.Nome)));
    }

    [Fact]
    public async Task CriacaoEEdicaoMantemUmUnicoRegistro()
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

            await using (var criacao = new GestaoOrcamentosDbContext(options))
            {
                var negocio = new ConfiguracaoNegocio { Id = 1, Nome = string.Empty };
                new ConfiguracaoFormulario { Nome = "  Primeiro nome  ", Email = "contato@exemplo.com" }
                    .AplicarEm(negocio);
                criacao.ConfiguracoesNegocio.Add(negocio);
                await criacao.SaveChangesAsync();
            }

            await using (var edicao = new GestaoOrcamentosDbContext(options))
            {
                var negocio = await edicao.ConfiguracoesNegocio.SingleAsync();
                Assert.Equal("Primeiro nome", negocio.Nome);

                var formulario = ConfiguracaoFormulario.DoNegocio(negocio);
                formulario.Nome = "  Nome atualizado  ";
                formulario.Cidade = "São Paulo";
                formulario.AplicarEm(negocio);
                await edicao.SaveChangesAsync();
            }

            await using (var confirmacao = new GestaoOrcamentosDbContext(options))
            {
                var negocio = await confirmacao.ConfiguracoesNegocio.AsNoTracking().SingleAsync();
                Assert.Equal("Nome atualizado", negocio.Nome);
                Assert.Equal("contato@exemplo.com", negocio.Email);
                Assert.Equal("São Paulo", negocio.Cidade);
            }

            await using (var duplicacao = new GestaoOrcamentosDbContext(options))
            {
                duplicacao.ConfiguracoesNegocio.Add(new ConfiguracaoNegocio { Id = 2, Nome = "Outro negócio" });
                await Assert.ThrowsAsync<DbUpdateException>(() => duplicacao.SaveChangesAsync());
            }
        }
        finally
        {
            await setup.Database.EnsureDeletedAsync();
        }
    }
}