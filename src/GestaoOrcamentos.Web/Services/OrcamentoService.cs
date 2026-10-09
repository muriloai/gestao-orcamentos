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
        var itens = Validar(formulario);
        if (itens.Any(item => item.Id != 0))
        {
            throw new ValidationException("Os itens de um novo orçamento não podem ter ID.");
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
            Itens = itens.Select(CriarItem).ToList()
        };

        dbContext.Orcamentos.Add(orcamento);
        await dbContext.SaveChangesAsync(cancellationToken);
        return orcamento.Id;
    }

    public async Task<bool> AtualizarAsync(int id, OrcamentoFormulario formulario, CancellationToken cancellationToken)
    {
        var orcamento = await dbContext.Orcamentos.Include(item => item.Itens)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (orcamento is null)
        {
            return false;
        }

        var itens = Validar(formulario);
        var idsAtuais = orcamento.Itens.Select(item => item.Id).ToHashSet();
        var idsInformados = itens.Where(item => item.Id != 0).Select(item => item.Id).ToList();
        if (idsInformados.Count != idsInformados.Distinct().Count() ||
            idsInformados.Any(itemId => !idsAtuais.Contains(itemId)))
        {
            throw new ValidationException("Um item informado não pertence a este orçamento.");
        }

        if (!await dbContext.Clientes.AnyAsync(cliente => cliente.Id == formulario.ClienteId, cancellationToken))
        {
            throw new InvalidOperationException("Cliente não encontrado.");
        }

        orcamento.ClienteId = formulario.ClienteId;
        orcamento.Titulo = formulario.Titulo.Trim();
        orcamento.DataEmissao = formulario.DataEmissao!.Value;
        orcamento.Validade = formulario.Validade;
        orcamento.Observacoes = formulario.Observacoes?.Trim();

        foreach (var item in orcamento.Itens.Where(item => !idsInformados.Contains(item.Id)).ToList())
        {
            orcamento.Itens.Remove(item);
            dbContext.ItensOrcamento.Remove(item);
        }

        foreach (var formularioItem in itens)
        {
            if (formularioItem.Id == 0)
            {
                orcamento.Itens.Add(CriarItem(formularioItem));
            }
            else
            {
                var item = orcamento.Itens.Single(item => item.Id == formularioItem.Id);
                item.Descricao = formularioItem.Descricao.Trim();
                item.Quantidade = formularioItem.Quantidade;
                item.Unidade = formularioItem.Unidade.Trim();
                item.PrecoUnitario = formularioItem.PrecoUnitario;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int?> ExcluirAsync(int id, CancellationToken cancellationToken)
    {
        var orcamento = await dbContext.Orcamentos.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (orcamento is null)
        {
            return null;
        }

        dbContext.Orcamentos.Remove(orcamento);
        await dbContext.SaveChangesAsync(cancellationToken);
        return orcamento.ClienteId;
    }

    private static List<ItemOrcamentoFormulario> Validar(OrcamentoFormulario formulario)
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

        return itens;
    }

    private static ItemOrcamento CriarItem(ItemOrcamentoFormulario item) => new()
    {
        Descricao = item.Descricao.Trim(),
        Quantidade = item.Quantidade,
        Unidade = item.Unidade.Trim(),
        PrecoUnitario = item.PrecoUnitario
    };
}