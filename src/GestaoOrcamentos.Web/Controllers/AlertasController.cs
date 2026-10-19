using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

public class AlertasController(AlertasOrcamentoService alertasService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Vencidos(CancellationToken cancellationToken)
    {
        var orcamentos = await alertasService.ListarVencidosAsync(cancellationToken);
        return View(new AvisosOrcamentos(alertasService.DataReferencia, orcamentos));
    }
}
