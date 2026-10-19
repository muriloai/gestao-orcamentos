using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using GestaoOrcamentos.Web.Models.Api;

namespace GestaoOrcamentos.Web.Services;

public class ViaCepClient(HttpClient httpClient)
{
    public async Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken cancellationToken)
    {
        using var resposta = await httpClient.GetAsync($"{cep}/json/", cancellationToken);
        if (!resposta.IsSuccessStatusCode)
        {
            throw new HttpRequestException("O ViaCEP retornou uma resposta de erro.", null, resposta.StatusCode);
        }

        var endereco = await resposta.Content.ReadFromJsonAsync<RespostaViaCep>(cancellationToken);
        if (endereco is null)
        {
            throw new InvalidDataException("A resposta do ViaCEP está vazia.");
        }

        if (endereco.Erro)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(endereco.Cep) ||
            string.IsNullOrWhiteSpace(endereco.Cidade) ||
            string.IsNullOrWhiteSpace(endereco.Uf))
        {
            throw new InvalidDataException("A resposta do ViaCEP não contém um endereço completo.");
        }

        return new EnderecoCepDto(endereco.Cep, endereco.Logradouro, endereco.Complemento,
            endereco.Bairro, endereco.Cidade, endereco.Uf);
    }

    private sealed record RespostaViaCep(
        [property: JsonPropertyName("cep")] string? Cep,
        [property: JsonPropertyName("logradouro")] string? Logradouro,
        [property: JsonPropertyName("complemento")] string? Complemento,
        [property: JsonPropertyName("bairro")] string? Bairro,
        [property: JsonPropertyName("localidade")] string? Cidade,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("erro")] bool Erro);
}