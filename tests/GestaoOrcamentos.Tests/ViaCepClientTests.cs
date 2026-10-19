using System.Net;
using System.Text;

using GestaoOrcamentos.Web.Services;

namespace GestaoOrcamentos.Tests;

public class ViaCepClientTests
{
    [Fact]
    public async Task ConverteRespostaDoViaCepParaDtoDaAplicacao()
    {
        using var httpClient = CriarCliente("""
            {"cep":"01310-100","logradouro":"Avenida Paulista","complemento":"de 1000 a 1998 - lado par","bairro":"Bela Vista","localidade":"São Paulo","uf":"SP","erro":false}
            """);
        var client = new ViaCepClient(httpClient);

        var endereco = await client.ConsultarAsync("01310100", CancellationToken.None);

        Assert.NotNull(endereco);
        Assert.Equal("01310-100", endereco.Cep);
        Assert.Equal("Avenida Paulista", endereco.Logradouro);
        Assert.Equal("de 1000 a 1998 - lado par", endereco.Complemento);
        Assert.Equal("Bela Vista", endereco.Bairro);
        Assert.Equal("São Paulo", endereco.Cidade);
        Assert.Equal("SP", endereco.Uf);
    }

    [Fact]
    public async Task CepInexistenteRetornaNulo()
    {
        using var httpClient = CriarCliente("""{"erro":true}""");
        var client = new ViaCepClient(httpClient);

        Assert.Null(await client.ConsultarAsync("00000000", CancellationToken.None));
    }

    [Fact]
    public async Task JsonInvalidoFalha()
    {
        using var httpClient = CriarCliente("not-json");
        var client = new ViaCepClient(httpClient);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => client.ConsultarAsync("01310100", CancellationToken.None));
    }

    [Fact]
    public async Task RespostaSemDadosObrigatoriosFalha()
    {
        using var httpClient = CriarCliente("{}");
        var client = new ViaCepClient(httpClient);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => client.ConsultarAsync("01310100", CancellationToken.None));
    }

    [Fact]
    public async Task ErroHttpDoProvedorEPropagado()
    {
        using var httpClient = new HttpClient(new RespostaHandler(new HttpResponseMessage(HttpStatusCode.BadGateway)))
        {
            BaseAddress = new Uri("https://viacep.test/ws/")
        };
        var client = new ViaCepClient(httpClient);

        var erro = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.ConsultarAsync("01310100", CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadGateway, erro.StatusCode);
    }

    private static HttpClient CriarCliente(string conteudo) => new(new RespostaHandler(
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(conteudo, Encoding.UTF8, "application/json")
        }))
    {
        BaseAddress = new Uri("https://viacep.test/ws/")
    };

    private sealed class RespostaHandler(HttpResponseMessage resposta) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(resposta);
    }
}