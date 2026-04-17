// -----------------------------------------------------------------------------
// 1.2 – Migration: Adds index on PRODUCT_NAME column
// -----------------------------------------------------------------------------
// This migration was created after adding HasIndex() in AppDbContext.
// It adds an index named "IX_PRODUCTS_NAME" on the PRODUCT_NAME column.
// This index will speed up queries that search for product names using prefix
// patterns (e.g., WHERE PRODUCT_NAME LIKE 'A%').
// The index is created when dotnet ef database update is run.
// This represents the "after" state for our indexing performance comparison.
// -----------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OracleDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOnProductName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PRODUCT_NAME",
                table: "PRODUCTS",
                type: "NVARCHAR2(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRICE",
                table: "PRODUCTS",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");
            // =====================================================
            // 1. Database Indexing Impact Analysis
            // This migration adds the index on ProductName (IX_PRODUCTS_NAME).
            // Related file: Data/AppDbContext.cs (index configuration)
            // =====================================================
            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_NAME",
                table: "PRODUCTS",
                column: "PRODUCT_NAME");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index if we roll back this migration
            migrationBuilder.DropIndex(
                name: "IX_PRODUCTS_NAME",
                table: "PRODUCTS");

            migrationBuilder.AlterColumn<string>(
                name: "PRODUCT_NAME",
                table: "PRODUCTS",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRICE",
                table: "PRODUCTS",
                type: "DECIMAL(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18, 2)");
        }
    }
}
