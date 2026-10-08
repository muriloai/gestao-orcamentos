using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

namespace GestaoOrcamentos.Web.Services;

public class ClienteService(GestaoOrcamentosDbContext dbContext)
{
    public async Task<int> CadastrarAsync(ClienteCadastro cadastro, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(cadastro, new ValidationContext(cadastro), validateAllProperties: true);

        var cliente = new Cliente
        {
            Nome = cadastro.Nome.Trim(),
            PessoaContato = cadastro.PessoaContato?.Trim(),
            Telefone = cadastro.Telefone?.Trim(),
            Email = cadastro.Email?.Trim(),
            Cep = cadastro.Cep?.Trim(),
            Logradouro = cadastro.Logradouro?.Trim(),
            Numero = cadastro.Numero?.Trim(),
            Complemento = cadastro.Complemento?.Trim(),
            Bairro = cadastro.Bairro?.Trim(),
            Cidade = cadastro.Cidade?.Trim(),
            Uf = cadastro.Uf?.Trim(),
            Observacoes = cadastro.Observacoes?.Trim()
        };

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }
}