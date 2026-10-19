namespace GestaoOrcamentos.Web.Models.Api;

public record EnderecoCepDto(
    string Cep,
    string? Logradouro,
    string? Complemento,
    string? Bairro,
    string Cidade,
    string Uf);