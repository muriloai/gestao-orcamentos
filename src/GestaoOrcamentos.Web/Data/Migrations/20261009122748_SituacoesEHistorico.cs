using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoOrcamentos.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SituacoesEHistorico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Bairro",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Cep",
                table: "Orcamentos",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Cidade",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Complemento",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Email",
                table: "Orcamentos",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Logradouro",
                table: "Orcamentos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Nome",
                table: "Orcamentos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Numero",
                table: "Orcamentos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Observacoes",
                table: "Orcamentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_PessoaContato",
                table: "Orcamentos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Telefone",
                table: "Orcamentos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteRegistrado_Uf",
                table: "Orcamentos",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Bairro",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Cep",
                table: "Orcamentos",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Cidade",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Complemento",
                table: "Orcamentos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Email",
                table: "Orcamentos",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Logradouro",
                table: "Orcamentos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Nome",
                table: "Orcamentos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Numero",
                table: "Orcamentos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Observacoes",
                table: "Orcamentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_PessoaContato",
                table: "Orcamentos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Telefone",
                table: "Orcamentos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NegocioRegistrado_Uf",
                table: "Orcamentos",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Situacao",
                table: "Orcamentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orcamentos_DadosRegistrados",
                table: "Orcamentos",
                sql: "([Situacao] = 0 AND [ClienteRegistrado_Nome] IS NULL AND [NegocioRegistrado_Nome] IS NULL) OR ([Situacao] IN (1, 2, 3) AND [ClienteRegistrado_Nome] IS NOT NULL AND [NegocioRegistrado_Nome] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orcamentos_Situacao",
                table: "Orcamentos",
                sql: "[Situacao] IN (0, 1, 2, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orcamentos_DadosRegistrados",
                table: "Orcamentos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orcamentos_Situacao",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Bairro",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Cep",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Cidade",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Complemento",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Email",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Logradouro",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Nome",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Numero",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Observacoes",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_PessoaContato",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Telefone",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "ClienteRegistrado_Uf",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Bairro",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Cep",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Cidade",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Complemento",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Email",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Logradouro",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Nome",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Numero",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Observacoes",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_PessoaContato",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Telefone",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "NegocioRegistrado_Uf",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "Situacao",
                table: "Orcamentos");
        }
    }
}