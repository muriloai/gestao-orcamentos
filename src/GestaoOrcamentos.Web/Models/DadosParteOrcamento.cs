namespace GestaoOrcamentos.Web.Models;

public class DadosParteOrcamento
{
    public required string Nome { get; set; }
    public string? PessoaContato { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Observacoes { get; set; }

    public static DadosParteOrcamento DoCliente(Cliente cliente) => new()
    {
        Nome = cliente.Nome,
        PessoaContato = cliente.PessoaContato,
        Telefone = cliente.Telefone,
        Email = cliente.Email,
        Cep = cliente.Cep,
        Logradouro = cliente.Logradouro,
        Numero = cliente.Numero,
        Complemento = cliente.Complemento,
        Bairro = cliente.Bairro,
        Cidade = cliente.Cidade,
        Uf = cliente.Uf,
        Observacoes = cliente.Observacoes
    };

    public static DadosParteOrcamento DoNegocio(ConfiguracaoNegocio negocio) => new()
    {
        Nome = negocio.Nome,
        Telefone = negocio.Telefone,
        Email = negocio.Email,
        Cep = negocio.Cep,
        Logradouro = negocio.Logradouro,
        Numero = negocio.Numero,
        Complemento = negocio.Complemento,
        Bairro = negocio.Bairro,
        Cidade = negocio.Cidade,
        Uf = negocio.Uf
    };
}