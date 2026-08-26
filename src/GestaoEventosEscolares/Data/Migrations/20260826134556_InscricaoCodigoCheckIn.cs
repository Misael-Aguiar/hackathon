using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEventosEscolares.Data.Migrations
{
    /// <inheritdoc />
    public partial class InscricaoCodigoCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoCheckIn",
                table: "Inscricoes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            // Hash do Id+GUID, sem 0/1 (alfabeto do código curto). Garante valor único antes do índice.
            migrationBuilder.Sql("""
                UPDATE [Inscricoes]
                SET [CodigoCheckIn] = UPPER(SUBSTRING(
                    TRANSLATE(
                        CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT(CAST([Id] AS varchar(20)), N'|', [CodigoQr])), 2),
                        N'01',
                        N'23'),
                    1, 8))
                WHERE [CodigoCheckIn] IS NULL OR [CodigoCheckIn] = N'';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Inscricoes_CodigoCheckIn",
                table: "Inscricoes",
                column: "CodigoCheckIn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inscricoes_CodigoCheckIn",
                table: "Inscricoes");

            migrationBuilder.DropColumn(
                name: "CodigoCheckIn",
                table: "Inscricoes");
        }
    }
}
