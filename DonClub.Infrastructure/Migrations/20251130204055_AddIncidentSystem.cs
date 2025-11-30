using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Incidents",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagerId = table.Column<long>(type: "bigint", nullable: false),
                    SessionId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "app",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "app",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedByUserId",
                schema: "app",
                table: "Incidents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ManagerId",
                schema: "app",
                table: "Incidents",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ReviewedByUserId",
                schema: "app",
                table: "Incidents",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_SessionId",
                schema: "app",
                table: "Incidents",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidents",
                schema: "app");
        }
    }
}
