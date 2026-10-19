using System.Data;

using GestaoOrcamentos.Web.Data;
using GestaoOrcamentos.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace GestaoOrcamentos.Web.Services;

public class RelatorioOrcamentosService(GestaoOrcamentosDbContext dbContext)
{
    public async Task<IReadOnlyList<RelatorioOrcamentoPorSituacao>> ConsultarPeriodoAsync(
        DateOnly dataInicial, DateOnly dataFinal, CancellationToken cancellationToken)
    {
        if (dataInicial > dataFinal)
        {
            throw new ArgumentOutOfRangeException(nameof(dataInicial), "A data inicial deve ser anterior à data final.");
        }

        var conexao = dbContext.Database.GetDbConnection();
        var abriuConexao = conexao.State != ConnectionState.Open;
        if (abriuConexao)
        {
            await conexao.OpenAsync(cancellationToken);
        }

        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = "dbo.sp_RelatorioOrcamentosPorPeriodo";
            comando.CommandType = CommandType.StoredProcedure;

            var inicio = comando.CreateParameter();
            inicio.ParameterName = "@DataInicial";
            inicio.DbType = DbType.Date;
            inicio.Value = dataInicial.ToDateTime(TimeOnly.MinValue);
            comando.Parameters.Add(inicio);

            var fim = comando.CreateParameter();
            fim.ParameterName = "@DataFinal";
            fim.DbType = DbType.Date;
            fim.Value = dataFinal.ToDateTime(TimeOnly.MinValue);
            comando.Parameters.Add(fim);

            var resultados = new List<RelatorioOrcamentoPorSituacao>();
            await using var leitor = await comando.ExecuteReaderAsync(cancellationToken);
            while (await leitor.ReadAsync(cancellationToken))
            {
                resultados.Add(new RelatorioOrcamentoPorSituacao(
                    leitor.GetInt32(0),
                    leitor.GetString(1),
                    leitor.GetInt32(2),
                    leitor.GetDecimal(3)));
            }

            return resultados;
        }
        finally
        {
            if (abriuConexao)
            {
                await conexao.CloseAsync();
            }
        }
    }
}
