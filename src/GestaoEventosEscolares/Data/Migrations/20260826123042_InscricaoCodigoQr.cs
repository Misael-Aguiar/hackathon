using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEventosEscolares.Data.Migrations
{
    /// <inheritdoc />
    public partial class InscricaoCodigoQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoQr",
                table: "Inscricoes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [Inscricoes]
                SET [CodigoQr] = LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), '-', ''))
                WHERE [CodigoQr] IS NULL OR [CodigoQr] = N'';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_CodigoQr",
                table: "Inscricoes",
                column: "CodigoQr",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inscricoes_CodigoQr",
                table: "Inscricoes");

            migrationBuilder.DropColumn(
                name: "CodigoQr",
                table: "Inscricoes");
        }
    }
}
