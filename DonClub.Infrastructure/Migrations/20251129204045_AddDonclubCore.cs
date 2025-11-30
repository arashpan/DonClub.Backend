using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDonclubCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RulesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinPlayers = table.Column<byte>(type: "tinyint", nullable: true),
                    MaxPlayers = table.Column<byte>(type: "tinyint", nullable: true),
                    ScoringSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "app",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRoles",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Team = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameRoles_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "app",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerCount = table.Column<byte>(type: "tinyint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenarios_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "app",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioRoles",
                schema: "app",
                columns: table => new
                {
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    GameRoleId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioRoles", x => new { x.ScenarioId, x.GameRoleId });
                    table.ForeignKey(
                        name: "FK_ScenarioRoles_GameRoles_GameRoleId",
                        column: x => x.GameRoleId,
                        principalSchema: "app",
                        principalTable: "GameRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScenarioRoles_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalSchema: "app",
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    ScenarioId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<long>(type: "bigint", nullable: true),
                    StartTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Tier = table.Column<byte>(type: "tinyint", nullable: false),
                    MaxPlayers = table.Column<byte>(type: "tinyint", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeRequestStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "app",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "app",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "app",
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalSchema: "app",
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    EnteredByManagerId = table.Column<long>(type: "bigint", nullable: false),
                    EnteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEditedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scores_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "app",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scores_Users_EnteredByManagerId",
                        column: x => x.EnteredByManagerId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Scores_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionPlayers",
                schema: "app",
                columns: table => new
                {
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    ReservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionPlayers", x => new { x.SessionId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_SessionPlayers_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "app",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionPlayers_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreAudits",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScoreId = table.Column<long>(type: "bigint", nullable: false),
                    OldValue = table.Column<int>(type: "int", nullable: true),
                    NewValue = table.Column<int>(type: "int", nullable: false),
                    ChangedById = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreAudits_Scores_ScoreId",
                        column: x => x.ScoreId,
                        principalSchema: "app",
                        principalTable: "Scores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameRoles_GameId_Name",
                schema: "app",
                table: "GameRoles",
                columns: new[] { "GameId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Name",
                schema: "app",
                table: "Games",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BranchId_Name",
                schema: "app",
                table: "Rooms",
                columns: new[] { "BranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioRoles_GameRoleId",
                schema: "app",
                table: "ScenarioRoles",
                column: "GameRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_GameId_Name",
                schema: "app",
                table: "Scenarios",
                columns: new[] { "GameId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreAudits_ScoreId",
                schema: "app",
                table: "ScoreAudits",
                column: "ScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_EnteredByManagerId",
                schema: "app",
                table: "Scores",
                column: "EnteredByManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_PlayerId",
                schema: "app",
                table: "Scores",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_SessionId_PlayerId",
                schema: "app",
                table: "Scores",
                columns: new[] { "SessionId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_PlayerId",
                schema: "app",
                table: "SessionPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_BranchId_StartTimeUtc",
                schema: "app",
                table: "Sessions",
                columns: new[] { "BranchId", "StartTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_GameId",
                schema: "app",
                table: "Sessions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ManagerId",
                schema: "app",
                table: "Sessions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoomId_StartTimeUtc_EndTimeUtc",
                schema: "app",
                table: "Sessions",
                columns: new[] { "RoomId", "StartTimeUtc", "EndTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ScenarioId",
                schema: "app",
                table: "Sessions",
                column: "ScenarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenarioRoles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ScoreAudits",
                schema: "app");

            migrationBuilder.DropTable(
                name: "SessionPlayers",
                schema: "app");

            migrationBuilder.DropTable(
                name: "GameRoles",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Scores",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Rooms",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Scenarios",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Games",
                schema: "app");
        }
    }
}
