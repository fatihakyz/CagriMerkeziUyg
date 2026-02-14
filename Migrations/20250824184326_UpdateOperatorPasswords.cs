using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOperatorPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 1,
                column: "Sifre",
                value: "admin123");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 2,
                column: "Sifre",
                value: "op123");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 3,
                column: "Sifre",
                value: "super123");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 4,
                column: "Sifre",
                value: "op123");

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 5,
                column: "Sifre",
                value: "op123");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
