using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinerKlinikSistemi.Migrations
{
    /// <inheritdoc />
    public partial class MusaitlikTablosuEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VeterinerMusaitlikler_Veterinerler_VeterinerId",
                table: "VeterinerMusaitlikler");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VeterinerMusaitlikler",
                table: "VeterinerMusaitlikler");

            migrationBuilder.RenameTable(
                name: "VeterinerMusaitlikler",
                newName: "VeterinerMusaitlikleri");

            migrationBuilder.RenameIndex(
                name: "IX_VeterinerMusaitlikler_VeterinerId",
                table: "VeterinerMusaitlikleri",
                newName: "IX_VeterinerMusaitlikleri_VeterinerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VeterinerMusaitlikleri",
                table: "VeterinerMusaitlikleri",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VeterinerMusaitlikleri_Veterinerler_VeterinerId",
                table: "VeterinerMusaitlikleri",
                column: "VeterinerId",
                principalTable: "Veterinerler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VeterinerMusaitlikleri_Veterinerler_VeterinerId",
                table: "VeterinerMusaitlikleri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VeterinerMusaitlikleri",
                table: "VeterinerMusaitlikleri");

            migrationBuilder.RenameTable(
                name: "VeterinerMusaitlikleri",
                newName: "VeterinerMusaitlikler");

            migrationBuilder.RenameIndex(
                name: "IX_VeterinerMusaitlikleri_VeterinerId",
                table: "VeterinerMusaitlikler",
                newName: "IX_VeterinerMusaitlikler_VeterinerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VeterinerMusaitlikler",
                table: "VeterinerMusaitlikler",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VeterinerMusaitlikler_Veterinerler_VeterinerId",
                table: "VeterinerMusaitlikler",
                column: "VeterinerId",
                principalTable: "Veterinerler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
