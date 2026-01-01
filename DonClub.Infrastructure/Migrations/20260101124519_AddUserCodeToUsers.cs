using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
	public partial class AddUserCodeToUsers : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "UserCode",
				schema: "app",
				table: "Users",
				type: "nvarchar(6)",
				maxLength: 6,
				nullable: true);

			// پر کردن UserCode برای کاربران قبلی (از 100000 به بالا)
			migrationBuilder.Sql(@"
WITH cte AS (
    SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS rn
    FROM [app].[Users]
    WHERE [UserCode] IS NULL
)
UPDATE u
SET [UserCode] = RIGHT('000000' + CAST(100000 + cte.rn - 1 AS varchar(6)), 6)
FROM [app].[Users] u
INNER JOIN cte ON u.Id = cte.Id;
");

			migrationBuilder.AlterColumn<string>(
				name: "UserCode",
				schema: "app",
				table: "Users",
				type: "nvarchar(6)",
				maxLength: 6,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(6)",
				oldMaxLength: 6,
				oldNullable: true);

			migrationBuilder.CreateIndex(
				name: "IX_Users_UserCode",
				schema: "app",
				table: "Users",
				column: "UserCode",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Users_UserCode",
				schema: "app",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "UserCode",
				schema: "app",
				table: "Users");
		}
	}
}
