using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Services;

public class OrcamentoService(GestaoOrcamentosDbContext dbContext)
{
    public Task<Orcamento?> ObterAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Orcamentos.AsNoTracking()
            .Include(orcamento => orcamento.Cliente)
            .Include(orcamento => orcamento.Itens)
            .SingleOrDefaultAsync(orcamento => orcamento.Id == id, cancellationToken);

    public async Task<int> CadastrarAsync(OrcamentoFormulario formulario, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(formulario, new ValidationContext(formulario), validateAllProperties: true);
        var itens = formulario.Itens ?? throw new ValidationException("Adicione pelo menos um item.");

        foreach (var item in itens)
        {
            Validator.ValidateObject(item, new ValidationContext(item), validateAllProperties: true);
            if (item.Quantidade != decimal.Round(item.Quantidade, 3) ||
                item.PrecoUnitario != decimal.Round(item.PrecoUnitario, 2))
            {
                throw new ValidationException("Quantidade ou preço possui casas decimais além do permitido.");
            }
        }

        if (!await dbContext.Clientes.AnyAsync(cliente => cliente.Id == formulario.ClienteId, cancellationToken))
        {
            throw new InvalidOperationException("Cliente não encontrado.");
        }

        var orcamento = new Orcamento
        {
            ClienteId = formulario.ClienteId,
            Titulo = formulario.Titulo.Trim(),
            DataEmissao = formulario.DataEmissao!.Value,
            Validade = formulario.Validade,
            Observacoes = formulario.Observacoes?.Trim(),
            Itens = itens.Select(item => new ItemOrcamento
            {
                Descricao = item.Descricao.Trim(),
                Quantidade = item.Quantidade,
                Unidade = item.Unidade.Trim(),
                PrecoUnitario = item.PrecoUnitario
            }).ToList()
        };

        dbContext.Orcamentos.Add(orcamento);
        await dbContext.SaveChangesAsync(cancellationToken);
        return orcamento.Id;
    }
}