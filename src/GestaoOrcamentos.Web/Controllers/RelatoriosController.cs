using GestaoOrcamentos.Web.Models;
using GestaoOrcamentos.Web.Services;

using Microsoft.AspNetCore.Mvc;

namespace GestaoOrcamentos.Web.Controllers;

public class RelatoriosController(RelatorioOrcamentosService relatorioService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Orcamentos(DateOnly? dataInicial, DateOnly? dataFinal,
        CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")));
        if (dataInicial is null && dataFinal is null)
        {
            dataInicial = new DateOnly(hoje.Year, hoje.Month, 1);
            dataFinal = hoje;
        }
        else if (dataInicial is null || dataFinal is null)
        {
            ModelState.AddModelError(nameof(dataInicial), "Informe as duas datas do período.");
        }
        else if (dataInicial > dataFinal)
        {
            ModelState.AddModelError(nameof(dataInicial), "A data inicial deve ser anterior ou igual à data final.");
        }

        var situacoes = ModelState.IsValid
            ? await relatorioService.ConsultarPeriodoAsync(dataInicial!.Value, dataFinal!.Value, cancellationToken)
            : [];

        return View(new RelatorioOrcamentos(dataInicial ?? hoje, dataFinal ?? hoje, situacoes));
    }
}
