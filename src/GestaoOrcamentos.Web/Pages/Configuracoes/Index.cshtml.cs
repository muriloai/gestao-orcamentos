using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Pages.Configuracoes;

public class IndexModel(GestaoOrcamentosDbContext dbContext) : PageModel
{
    [BindProperty]
    public ConfiguracaoFormulario Configuracao { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var negocio = await dbContext.ConfiguracoesNegocio.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);

        if (negocio is not null)
        {
            Configuracao = ConfiguracaoFormulario.DoNegocio(negocio);
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var negocio = await dbContext.ConfiguracoesNegocio
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);

        if (negocio is null)
        {
            negocio = new ConfiguracaoNegocio { Id = 1, Nome = Configuracao.Nome };
            dbContext.ConfiguracoesNegocio.Add(negocio);
        }

        Configuracao.AplicarEm(negocio);
        await dbContext.SaveChangesAsync(cancellationToken);

        TempData["Mensagem"] = "Configurações salvas com sucesso.";
        return RedirectToPage();
    }
}