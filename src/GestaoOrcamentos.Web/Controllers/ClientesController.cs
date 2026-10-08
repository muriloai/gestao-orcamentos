using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

public class ClientesController(ClienteService clienteService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, CancellationToken cancellationToken)
    {
        var clientes = await clienteService.ListarAsync(busca, cancellationToken);
        return View(new ClientesLista(busca, clientes));
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.ObterAsync(id, cancellationToken);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View(new ClienteFormulario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ClienteFormulario formulario, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(formulario);
        }

        var id = await clienteService.CadastrarAsync(formulario, cancellationToken);
        TempData["Mensagem"] = "Cliente cadastrado com sucesso.";

        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.ObterAsync(id, cancellationToken);
        return cliente is null ? NotFound() : View(ClienteFormulario.DoCliente(cliente));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ClienteFormulario formulario, CancellationToken cancellationToken)
    {
        formulario.Id = id;

        if (!ModelState.IsValid)
        {
            if (await clienteService.ObterAsync(id, cancellationToken) is null)
            {
                return NotFound();
            }

            return View(formulario);
        }

        if (!await clienteService.AtualizarAsync(id, formulario, cancellationToken))
        {
            return NotFound();
        }

        TempData["Mensagem"] = "Cliente atualizado com sucesso.";
        return RedirectToAction(nameof(Detalhes), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.ObterAsync(id, cancellationToken);
        return cliente is null ? NotFound() : View(cliente);
    }

    [HttpPost]
    [ActionName(nameof(Excluir))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarExclusao(int id, CancellationToken cancellationToken)
    {
        if (!await clienteService.ExcluirAsync(id, cancellationToken))
        {
            return NotFound();
        }

        TempData["Mensagem"] = "Cliente excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}