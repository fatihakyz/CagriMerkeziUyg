using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class OperatorPerformansSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CagriSuresi",
                table: "MusteriAktiviteleri",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CozumTarihi",
                table: "MusteriAktiviteleri",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "MusteriAktiviteleri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MusteriMemnuniyet",
                table: "MusteriAktiviteleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Oncelik",
                table: "MusteriAktiviteleri",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                table: "MusteriAktiviteleri",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Operatorler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonGiris = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalismaSaatiBaslangic = table.Column<TimeSpan>(type: "time", nullable: true),
                    CalismaSaatiBitis = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operatorler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatorPerformansKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CozulenCagriSayisi = table.Column<int>(type: "int", nullable: false),
                    ToplamCagriSayisi = table.Column<int>(type: "int", nullable: false),
                    OrtalamaCagriSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MusteriMemnuniyetPuani = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    AktifCalismaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ToplamMolaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PerformansPuani = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Notlar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorPerformansKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorPerformansKayitlari_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Operatorler",
                columns: new[] { "Id", "Ad", "Aktif", "CalismaSaatiBaslangic", "CalismaSaatiBitis", "Email", "KayitTarihi", "KullaniciAdi", "Rol", "SonGiris", "Soyad", "Telefon" },
                values: new object[,]
                {
                    { 1, "Admin", true, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 18, 0, 0, 0), "admin@cagrimerkezi.com", new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "admin", "Admin", null, "User", "05551111111" },
                    { 2, "Mehmet", true, new TimeSpan(0, 9, 0, 0, 0), new TimeSpan(0, 17, 0, 0, 0), "mehmet@cagrimerkezi.com", new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "mehmet.demir", "Operator", null, "Demir", "05552222222" },
                    { 3, "Fatma", true, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 17, 30, 0, 0), "fatma@cagrimerkezi.com", new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "fatma.ozkan", "Supervisor", null, "Özkan", "05553333333" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusteriAktiviteleri_OperatorId",
                table: "MusteriAktiviteleri",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Operatorler_Email",
                table: "Operatorler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Operatorler_KullaniciAdi",
                table: "Operatorler",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperatorPerformansKayitlari_OperatorId",
                table: "OperatorPerformansKayitlari",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorPerformansKayitlari_Tarih",
                table: "OperatorPerformansKayitlari",
                column: "Tarih");

            migrationBuilder.AddForeignKey(
                name: "FK_MusteriAktiviteleri_Operatorler_OperatorId",
                table: "MusteriAktiviteleri",
                column: "OperatorId",
                principalTable: "Operatorler",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusteriAktiviteleri_Operatorler_OperatorId",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropTable(
                name: "OperatorPerformansKayitlari");

            migrationBuilder.DropTable(
                name: "Operatorler");

            migrationBuilder.DropIndex(
                name: "IX_MusteriAktiviteleri_OperatorId",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "CagriSuresi",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "CozumTarihi",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "Durum",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "MusteriMemnuniyet",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "Oncelik",
                table: "MusteriAktiviteleri");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "MusteriAktiviteleri");
        }
    }
}
