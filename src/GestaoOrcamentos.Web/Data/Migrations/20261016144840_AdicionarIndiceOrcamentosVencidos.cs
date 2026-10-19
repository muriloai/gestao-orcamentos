using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoOrcamentos.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndiceOrcamentosVencidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_Pendente_Validade",
                table: "Orcamentos",
                columns: new[] { "Validade", "Id" },
                filter: "[Situacao] = 1 AND [Validade] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orcamentos_Pendente_Validade",
                table: "Orcamentos");
        }
    }
}
