using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sifre",
                table: "Operatorler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 1,
                column: "Sifre",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 2,
                column: "Sifre",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 3,
                column: "Sifre",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 4,
                column: "Sifre",
                value: "123456");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 5,
                column: "Sifre",
                value: "123456");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sifre",
                table: "Operatorler");
        }
    }
}
