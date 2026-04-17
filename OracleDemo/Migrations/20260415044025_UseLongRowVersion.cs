using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OracleDemo.Migrations
{
    /// <inheritdoc />
    public partial class UseLongRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RowVersion",
                table: "PRODUCTS",
                type: "NUMBER(19)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "RAW(8)",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PRICE",
                table: "PRODUCTS",
                type: "DECIMAL(18, 2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "DECIMAL(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PRODUCTS",
                type: "RAW(8)",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

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
