using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoOrcamentos.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CriarResumoOrcamentosSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER VIEW dbo.vw_ResumoOrcamentos
                AS
                SELECT
                    o.Id,
                    o.ClienteId,
                    CASE WHEN o.Situacao = 0 THEN c.Nome ELSE o.ClienteRegistrado_Nome END AS NomeCliente,
                    o.Titulo,
                    o.DataEmissao,
                    o.Validade,
                    o.Situacao,
                    CAST(COALESCE(
                        SUM(CAST(ROUND(i.Quantidade * i.PrecoUnitario, 2) AS decimal(18, 2))),
                        0) AS decimal(28, 2)) AS Total
                FROM dbo.Orcamentos AS o
                INNER JOIN dbo.Clientes AS c ON c.Id = o.ClienteId
                LEFT JOIN dbo.ItensOrcamento AS i ON i.OrcamentoId = o.Id
                GROUP BY o.Id, o.ClienteId, c.Nome, o.ClienteRegistrado_Nome,
                    o.Titulo, o.DataEmissao, o.Validade, o.Situacao;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.sp_RelatorioOrcamentosPorPeriodo
                    @DataInicial date,
                    @DataFinal date
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @DataInicial IS NULL OR @DataFinal IS NULL OR @DataInicial > @DataFinal
                        THROW 50001, 'O período informado é inválido.', 1;

                    SELECT
                        situacoes.Situacao,
                        situacoes.Descricao,
                        COUNT(resumo.Id) AS QuantidadeOrcamentos,
                        CAST(COALESCE(SUM(resumo.Total), 0) AS decimal(28, 2)) AS ValorTotal
                    FROM (VALUES
                        (0, N'Rascunhos'),
                        (1, N'Pendentes de aprovação'),
                        (2, N'Propostas aprovadas'),
                        (3, N'Propostas recusadas')
                    ) AS situacoes(Situacao, Descricao)
                    LEFT JOIN dbo.vw_ResumoOrcamentos AS resumo
                        ON resumo.Situacao = situacoes.Situacao
                        AND resumo.DataEmissao >= @DataInicial
                        AND resumo.DataEmissao <= @DataFinal
                    GROUP BY situacoes.Situacao, situacoes.Descricao;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_RelatorioOrcamentosPorPeriodo;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_ResumoOrcamentos;");
        }
    }
}
