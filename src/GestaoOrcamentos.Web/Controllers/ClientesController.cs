using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

public class ClientesController(ClienteService clienteService) : Controller
{
    [HttpGet]
    public IActionResult Criar()
    {
        return View(new ClienteCadastro());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ClienteCadastro cadastro, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(cadastro);
        }

        await clienteService.CadastrarAsync(cadastro, cancellationToken);
        TempData["Mensagem"] = "Cliente cadastrado com sucesso.";

        return RedirectToAction(nameof(Criar));
    }
}