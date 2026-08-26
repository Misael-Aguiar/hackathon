using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEventosEscolares.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventoCamposEPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PodeAcessarPresenca",
                table: "ProfessoresAutorizadosEvento",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PodeEditarEvento",
                table: "ProfessoresAutorizadosEvento",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Eventos",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "CaminhoImagem",
                table: "Eventos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InformacoesAdicionais",
                table: "Eventos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Objetivo",
                table: "Eventos",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subtitulo",
                table: "Eventos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PodeAcessarPresenca",
                table: "ProfessoresAutorizadosEvento");

            migrationBuilder.DropColumn(
                name: "PodeEditarEvento",
                table: "ProfessoresAutorizadosEvento");

            migrationBuilder.DropColumn(
                name: "CaminhoImagem",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "InformacoesAdicionais",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Objetivo",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Subtitulo",
                table: "Eventos");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Eventos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);
        }
    }
}
