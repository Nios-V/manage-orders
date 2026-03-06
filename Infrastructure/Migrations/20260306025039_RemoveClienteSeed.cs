using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClienteSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Ordenes_ClienteId",
                table: "Ordenes",
                newName: "ix_Ordenes_ClienteId");

            migrationBuilder.CreateIndex(
                name: "ix_Ordenes_FechaCreacion",
                table: "Ordenes",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "uix_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_Ordenes_FechaCreacion",
                table: "Ordenes");

            migrationBuilder.DropIndex(
                name: "uix_Clientes_Email",
                table: "Clientes");

            migrationBuilder.RenameIndex(
                name: "ix_Ordenes_ClienteId",
                table: "Ordenes",
                newName: "IX_Ordenes_ClienteId");
        }
    }
}
