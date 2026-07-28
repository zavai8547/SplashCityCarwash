using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashCityCarwash.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBulkEntry",
                table: "Transactions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VehicleCount",
                table: "Transactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBulkEntry",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "VehicleCount",
                table: "Transactions");
        }
    }
}
