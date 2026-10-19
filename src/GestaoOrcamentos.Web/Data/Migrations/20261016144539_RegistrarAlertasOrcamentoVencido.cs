using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoOrcamentos.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RegistrarAlertasOrcamentoVencido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR ALTER FUNCTION dbo.fn_OrcamentosVencidos(@DataReferencia date)
                RETURNS TABLE
                AS
                RETURN
                (
                    SELECT o.Id AS OrcamentoId
                    FROM dbo.Orcamentos AS o
                    WHERE o.Situacao = 1
                        AND o.Validade < @DataReferencia
                );
                """);

            migrationBuilder.CreateTable(
                name: "AlertasOrcamentosVencidos",
                columns: table => new
                {
                    OrcamentoId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasOrcamentosVencidos", x => x.OrcamentoId);
                    table.ForeignKey(
                        name: "FK_AlertasOrcamentosVencidos_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.fn_OrcamentosVencidos;");

            migrationBuilder.DropTable(
                name: "AlertasOrcamentosVencidos");
        }
    }
}
