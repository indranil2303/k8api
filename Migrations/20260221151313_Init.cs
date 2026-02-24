using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace k8api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(15,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("1bb52325-de91-4c9e-b7ad-c21139ee52f0"), "Prod III - B", 299.00m },
                    { new Guid("2c1d4256-cafd-4f1f-a50f-dc345675a559"), "Prod I - B", 149.99m },
                    { new Guid("4b10621a-bb5a-42da-9d1d-54f277190670"), "Prod VI", 199.00m },
                    { new Guid("4b3459ce-1da0-4741-b1c4-55715228b5ff"), "Prod IV - A", 199.99m },
                    { new Guid("7eb56574-27fe-4bc6-9b4c-e3553fb5e072"), "Prod V", 89.99m },
                    { new Guid("84e88a13-fa68-4eff-b622-bc0a4f625099"), "Prod I - A", 99.00m },
                    { new Guid("bfebbd3c-cf5d-43c2-8fbd-96b5b9a6f18e"), "Prod II", 49.99m },
                    { new Guid("edeaa402-a87f-4b3b-8a27-092505f199be"), "Prod III", 79.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
