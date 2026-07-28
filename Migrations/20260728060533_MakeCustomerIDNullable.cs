using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashCityCarwash.Migrations
{
    /// <inheritdoc />
    public partial class MakeCustomerIDNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Customers_CustomerID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Vehicles_VehicleID",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleID",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Customers_CustomerID",
                table: "Transactions",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustomerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Vehicles_VehicleID",
                table: "Transactions",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Customers_CustomerID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Vehicles_VehicleID",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleID",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Customers_CustomerID",
                table: "Transactions",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Vehicles_VehicleID",
                table: "Transactions",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
