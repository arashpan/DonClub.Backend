using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DonClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardWalletAmountToBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RewardWalletAmount",
                schema: "app",
                table: "Badges",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RewardWalletAmount",
                schema: "app",
                table: "Badges");
        }
    }
}
