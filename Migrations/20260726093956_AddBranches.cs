using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplashCityCarwash.Migrations
{
    /// <inheritdoc />
    public partial class AddBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "ShopSales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchID",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagerName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BranchID",
                table: "Transactions",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_BranchID",
                table: "ShopSales",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BranchID",
                table: "Expenses",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchID",
                table: "AspNetUsers",
                column: "BranchID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchID",
                table: "AspNetUsers",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchID");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Branches_BranchID",
                table: "Expenses",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopSales_Branches_BranchID",
                table: "ShopSales",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Branches_BranchID",
                table: "Transactions",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Branches_BranchID",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopSales_Branches_BranchID",
                table: "ShopSales");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Branches_BranchID",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BranchID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ShopSales_BranchID",
                table: "ShopSales");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BranchID",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BranchID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "ShopSales");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BranchID",
                table: "AspNetUsers");
        }
    }
}
