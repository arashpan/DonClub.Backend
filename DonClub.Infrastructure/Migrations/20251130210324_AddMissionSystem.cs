using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissionDefinitions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Period = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    RewardWalletAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RewardDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ConditionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMissions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MissionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentValue = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProgressAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMissions_MissionDefinitions_MissionDefinitionId",
                        column: x => x.MissionDefinitionId,
                        principalSchema: "app",
                        principalTable: "MissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMissions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionDefinitions_Code",
                schema: "app",
                table: "MissionDefinitions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserMissions_MissionDefinitionId",
                schema: "app",
                table: "UserMissions",
                column: "MissionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMissions_UserId_MissionDefinitionId_PeriodStartUtc_PeriodEndUtc",
                schema: "app",
                table: "UserMissions",
                columns: new[] { "UserId", "MissionDefinitionId", "PeriodStartUtc", "PeriodEndUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMissions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "MissionDefinitions",
                schema: "app");
        }
    }
}
