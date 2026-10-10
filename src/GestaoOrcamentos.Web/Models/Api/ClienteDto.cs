namespace GestaoOrcamentos.Web.Models.Api;

public record ClienteDto(
    int Id,
    string Nome,
    string? PessoaContato,
    string? Telefone,
    string? Email,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacoes)
{
    public static ClienteDto DeCliente(Cliente cliente) => new(
        cliente.Id, cliente.Nome, cliente.PessoaContato, cliente.Telefone, cliente.Email,
        cliente.Cep, cliente.Logradouro, cliente.Numero, cliente.Complemento,
        cliente.Bairro, cliente.Cidade, cliente.Uf, cliente.Observacoes);
}

public record ClientesPaginaDto(
    List<ClienteDto> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);