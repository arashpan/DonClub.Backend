using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalGameRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameRoles_GameId_Name",
                schema: "app",
                table: "GameRoles");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                schema: "app",
                table: "GameRoles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_GameRoles_GameId_Name",
                schema: "app",
                table: "GameRoles",
                columns: new[] { "GameId", "Name" },
                unique: true,
                filter: "[GameId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GameRoles_Name",
                schema: "app",
                table: "GameRoles",
                column: "Name",
                unique: true,
                filter: "[GameId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameRoles_GameId_Name",
                schema: "app",
                table: "GameRoles");

            migrationBuilder.DropIndex(
                name: "IX_GameRoles_Name",
                schema: "app",
                table: "GameRoles");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                schema: "app",
                table: "GameRoles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRoles_GameId_Name",
                schema: "app",
                table: "GameRoles",
                columns: new[] { "GameId", "Name" },
                unique: true);
        }
    }
}
