using System.Data;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Services;

public class AlertasOrcamentoService(GestaoOrcamentosDbContext dbContext, TimeProvider timeProvider)
{
    private static readonly TimeZoneInfo FusoHorarioSaoPaulo =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateOnly DataReferencia => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), FusoHorarioSaoPaulo).DateTime);

    public async Task<int> RegistrarVencidosAsync(CancellationToken cancellationToken)
    {
        var conexao = dbContext.Database.GetDbConnection();
        var abriuConexao = conexao.State != ConnectionState.Open;
        if (abriuConexao)
        {
            await conexao.OpenAsync(cancellationToken);
        }

        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = """
                INSERT INTO dbo.AlertasOrcamentosVencidos (OrcamentoId, CriadoEm)
                SELECT vencidos.OrcamentoId, @Instante
                FROM dbo.fn_OrcamentosVencidos(@DataReferencia) AS vencidos
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.AlertasOrcamentosVencidos AS alerta WITH (UPDLOCK, HOLDLOCK)
                    WHERE alerta.OrcamentoId = vencidos.OrcamentoId);
                """;

            var instante = comando.CreateParameter();
            instante.ParameterName = "@Instante";
            instante.DbType = DbType.DateTimeOffset;
            instante.Value = timeProvider.GetUtcNow();
            comando.Parameters.Add(instante);

            var referencia = comando.CreateParameter();
            referencia.ParameterName = "@DataReferencia";
            referencia.DbType = DbType.Date;
            referencia.Value = DataReferencia.ToDateTime(TimeOnly.MinValue);
            comando.Parameters.Add(referencia);

            return await comando.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (abriuConexao)
            {
                await conexao.CloseAsync();
            }
        }
    }

    public Task<List<ResumoOrcamento>> ListarVencidosAsync(CancellationToken cancellationToken)
    {
        var hoje = DataReferencia;
        return (from resumo in dbContext.ResumosOrcamentos.AsNoTracking()
                join alerta in dbContext.AlertasOrcamentosVencidos.AsNoTracking()
                    on resumo.Id equals alerta.OrcamentoId
                where resumo.Situacao == SituacaoOrcamento.Pendente && resumo.Validade < hoje
                orderby resumo.Validade, resumo.Id
                select resumo)
            .ToListAsync(cancellationToken);
    }
}
