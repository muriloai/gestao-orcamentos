using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Services;

public class ClienteService(GestaoOrcamentosDbContext dbContext)
{
    public async Task<List<Cliente>> ListarAsync(string? busca, CancellationToken cancellationToken)
    {
        var consulta = dbContext.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(cliente => cliente.Nome.Contains(termo));
        }

        return await consulta.OrderBy(cliente => cliente.Nome)
            .ThenBy(cliente => cliente.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Cliente?> ObterAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Clientes.AsNoTracking().SingleOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);

    public async Task<int> CadastrarAsync(ClienteFormulario formulario, CancellationToken cancellationToken)
    {
        Validar(formulario);

        var cliente = new Cliente
        {
            Nome = formulario.Nome
        };
        AplicarDados(cliente, formulario);

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }

    public async Task<bool> AtualizarAsync(int id, ClienteFormulario formulario, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return false;
        }

        Validar(formulario);
        AplicarDados(cliente, formulario);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return false;
        }

        if (await dbContext.Orcamentos.AnyAsync(orcamento => orcamento.ClienteId == id, cancellationToken))
        {
            throw new InvalidOperationException("Não é possível excluir um cliente com orçamentos vinculados.");
        }

        dbContext.Clientes.Remove(cliente);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new InvalidOperationException("Não é possível excluir um cliente com orçamentos vinculados.", ex);
        }

        return true;
    }

    private static void Validar(ClienteFormulario formulario) =>
        Validator.ValidateObject(formulario, new ValidationContext(formulario), validateAllProperties: true);

    private static void AplicarDados(Cliente cliente, ClienteFormulario formulario)
    {
        cliente.Nome = formulario.Nome.Trim();
        cliente.PessoaContato = formulario.PessoaContato?.Trim();
        cliente.Telefone = formulario.Telefone?.Trim();
        cliente.Email = formulario.Email?.Trim();
        cliente.Cep = formulario.Cep?.Trim();
        cliente.Logradouro = formulario.Logradouro?.Trim();
        cliente.Numero = formulario.Numero?.Trim();
        cliente.Complemento = formulario.Complemento?.Trim();
        cliente.Bairro = formulario.Bairro?.Trim();
        cliente.Cidade = formulario.Cidade?.Trim();
        cliente.Uf = formulario.Uf?.Trim();
        cliente.Observacoes = formulario.Observacoes?.Trim();
    }
}