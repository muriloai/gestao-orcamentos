using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestaoOrcamentos.Web.Services;

public sealed class AlertaOrcamentoWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<AlertaOrcamentoWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var temporizador = new PeriodicTimer(TimeSpan.FromMinutes(1), timeProvider);
        do
        {
            await ExecutarCicloAsync(stoppingToken);
        }
        while (await temporizador.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExecutarCicloAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var escopo = scopeFactory.CreateAsyncScope();
            var servico = escopo.ServiceProvider.GetRequiredService<AlertasOrcamentoService>();
            var novosAvisos = await servico.RegistrarVencidosAsync(cancellationToken);
            if (novosAvisos > 0)
            {
                logger.LogInformation("Foram registrados {Quantidade} novos avisos de orçamento vencido.", novosAvisos);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Não foi possível verificar os orçamentos vencidos nesta execução.");
        }
    }
}
