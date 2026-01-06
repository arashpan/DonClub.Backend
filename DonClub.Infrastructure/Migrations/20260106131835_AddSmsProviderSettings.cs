using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsProviderSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsProviderSettings",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UseBaseServiceNumber = table.Column<bool>(type: "bit", nullable: false),
                    BodyId = table.Column<int>(type: "int", nullable: true),
                    IsFlash = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsProviderSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "SmsProviderSettings",
                columns: new[] { "Id", "ApiBaseUrl", "BodyId", "CreatedAtUtc", "Description", "FromNumber", "IsActive", "IsEnabled", "IsFlash", "Password", "Provider", "UpdatedAtUtc", "UseBaseServiceNumber", "Username" },
                values: new object[] { 1, "https://rest.payamak-panel.com/api/SendSMS/", null, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Seed اولیه تنظیمات پیامک‌رسان (ملی پیامک) - لطفاً مقادیر را تغییر دهید.", "5000XXXX", true, false, false, "CHANGE_ME", "Melipayamak", null, false, "CHANGE_ME" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsProviderSettings_IsActive",
                schema: "app",
                table: "SmsProviderSettings",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsProviderSettings",
                schema: "app");
        }
    }
}
