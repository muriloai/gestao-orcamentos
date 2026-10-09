using System.ComponentModel.DataAnnotations;

using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestaoOrcamentos.Web.Controllers;

public class OrcamentosController(OrcamentoService orcamentoService, ClienteService clienteService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Criar(int? clienteId, CancellationToken cancellationToken)
    {
        await CarregarClientesAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")));

        return View(new OrcamentoFormulario { ClienteId = clienteId ?? 0, DataEmissao = hoje });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(OrcamentoFormulario formulario, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid && await clienteService.ObterAsync(formulario.ClienteId, cancellationToken) is null)
        {
            ModelState.AddModelError(nameof(formulario.ClienteId), "Cliente não encontrado.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var id = await orcamentoService.CadastrarAsync(formulario, cancellationToken);
                TempData["Mensagem"] = "Orçamento cadastrado com sucesso.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(nameof(formulario.ClienteId), "Cliente não encontrado.");
            }
        }

        await CarregarClientesAsync(cancellationToken);
        return View(formulario);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterAsync(id, cancellationToken);
        return orcamento is null ? NotFound() : View(orcamento);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterAsync(id, cancellationToken);
        if (orcamento is null)
        {
            return NotFound();
        }

        await CarregarClientesAsync(cancellationToken);
        return View(OrcamentoFormulario.DoOrcamento(orcamento));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, OrcamentoFormulario formulario, CancellationToken cancellationToken)
    {
        formulario.Id = id;
        if (await orcamentoService.ObterAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        if (ModelState.IsValid && await clienteService.ObterAsync(formulario.ClienteId, cancellationToken) is null)
        {
            ModelState.AddModelError(nameof(formulario.ClienteId), "Cliente não encontrado.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (!await orcamentoService.AtualizarAsync(id, formulario, cancellationToken))
                {
                    return NotFound();
                }

                TempData["Mensagem"] = "Orçamento atualizado com sucesso.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(nameof(formulario.ClienteId), "Cliente não encontrado.");
            }
        }

        await CarregarClientesAsync(cancellationToken);
        return View(formulario);
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterAsync(id, cancellationToken);
        return orcamento is null ? NotFound() : View(orcamento);
    }

    [HttpPost]
    [ActionName(nameof(Excluir))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarExclusao(int id, CancellationToken cancellationToken)
    {
        var clienteId = await orcamentoService.ExcluirAsync(id, cancellationToken);
        if (clienteId is null)
        {
            return NotFound();
        }

        TempData["Mensagem"] = "Orçamento excluído com sucesso.";
        return RedirectToAction("Detalhes", "Clientes", new { id = clienteId });
    }

    private async Task CarregarClientesAsync(CancellationToken cancellationToken)
    {
        ViewBag.Clientes = (await clienteService.ListarAsync(null, cancellationToken))
            .Select(cliente => new SelectListItem(cliente.Nome, cliente.Id.ToString()))
            .ToList();
    }
}