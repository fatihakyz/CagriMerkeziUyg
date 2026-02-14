using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class DemoKullaniciEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 3,
                column: "KullaniciAdi",
                value: "supervisor");

            migrationBuilder.InsertData(
                table: "Operatorler",
                columns: new[] { "Id", "Ad", "Aktif", "CalismaSaatiBaslangic", "CalismaSaatiBitis", "Email", "KayitTarihi", "KullaniciAdi", "Rol", "SonGiris", "Soyad", "Telefon" },
                values: new object[,]
                {
                    { 4, "Ali", true, new TimeSpan(0, 9, 0, 0, 0), new TimeSpan(0, 18, 0, 0, 0), "ali@cagrimerkezi.com", new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "operator1", "Operator", null, "Kaya", "05554444444" },
                    { 5, "Zeynep", true, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 17, 30, 0, 0), "zeynep@cagrimerkezi.com", new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "operator2", "Operator", null, "Çelik", "05555555555" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 3,
                column: "KullaniciAdi",
                value: "fatma.ozkan");
        }
    }
}
