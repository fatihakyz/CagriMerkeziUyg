using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TelefonNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Müşteri telefon numarası"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Müşteri adı"),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Müşteri soyadı"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Müşteri e-posta adresi"),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Müşteri adresi"),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Müşteri doğum tarihi"),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Kayıt oluşturulma tarihi"),
                    SonGuncelleme = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Son güncelleme tarihi"),
                    Notlar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Müşteri hakkında notlar")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Musteriler",
                columns: new[] { "Id", "Ad", "Adres", "DogumTarihi", "Email", "KayitTarihi", "Notlar", "SonGuncelleme", "Soyad", "TelefonNo" },
                values: new object[,]
                {
                    { 1, "Ahmet", "İstanbul, Türkiye", new DateTime(1980, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "ahmet@email.com", new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5101), "Düzenli müşteri", new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5355), "Yılmaz", "05551234567" },
                    { 2, "Ayşe", "Ankara, Türkiye", new DateTime(1990, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayse@email.com", new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5874), "Yeni müşteri", new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5874), "Kaya", "05559876543" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_Email",
                table: "Musteriler",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Musteriler_TelefonNo",
                table: "Musteriler",
                column: "TelefonNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Musteriler");
        }
    }
}
