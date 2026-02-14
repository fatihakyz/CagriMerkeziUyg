using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class OperatorDurumYonetimi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CevapSablonKullanimlar_Musteriler_MusteriId",
                table: "CevapSablonKullanimlar");

            migrationBuilder.AddColumn<string>(
                name: "DurumNotu",
                table: "Operatorler",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GununBaslangicSaati",
                table: "Operatorler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GununBitisSaati",
                table: "Operatorler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MevcutDurum",
                table: "Operatorler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SonDurumDegisikliği",
                table: "Operatorler",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperatorDurumGecmisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    OncekiDurum = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Önceki durum"),
                    YeniDurum = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Yeni durum"),
                    GecisZamani = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Durum geçiş zamanı"),
                    BitisZamani = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Durum bitiş zamanı"),
                    Not = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Durum değişikliği notu"),
                    OtomatikGecis = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Otomatik geçiş mi?"),
                    IlgiliAramaLogId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorDurumGecmisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorDurumGecmisleri_AramaLoglari_IlgiliAramaLogId",
                        column: x => x.IlgiliAramaLogId,
                        principalTable: "AramaLoglari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OperatorDurumGecmisleri_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatorGunlukDurumOzetleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Özet tarihi"),
                    ToplamCalismaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Toplam çalışma süresi (dakika)"),
                    CagrıdaGecenSure = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Çağrıda geçen süre (dakika)"),
                    AraCalismaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Ara çalışma süresi (dakika)"),
                    MusaitSure = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Müsait bekleme süresi (dakika)"),
                    MolaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Mola süresi (dakika)"),
                    OgleYemegiSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Öğle yemeği süresi (dakika)"),
                    ToplantiSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Toplantı süresi (dakika)"),
                    EgitimSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, comment: "Eğitim süresi (dakika)"),
                    ToplamCagriSayisi = table.Column<int>(type: "int", nullable: false),
                    OrtalamaCagriSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, comment: "Ortalama çağrı süresi (dakika)"),
                    OrtalamaAraCalismaSuresi = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true, comment: "Ortalama ara çalışma süresi (dakika)"),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonGuncelleme = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorGunlukDurumOzetleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorGunlukDurumOzetleri_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DurumNotu", "GununBaslangicSaati", "GununBitisSaati", "MevcutDurum", "SonDurumDegisikliği" },
                values: new object[] { null, null, null, 0, null });

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DurumNotu", "GununBaslangicSaati", "GununBitisSaati", "MevcutDurum", "SonDurumDegisikliği" },
                values: new object[] { null, null, null, 0, null });

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DurumNotu", "GununBaslangicSaati", "GununBitisSaati", "MevcutDurum", "SonDurumDegisikliği" },
                values: new object[] { null, null, null, 0, null });

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DurumNotu", "GununBaslangicSaati", "GununBitisSaati", "MevcutDurum", "SonDurumDegisikliği" },
                values: new object[] { null, null, null, 0, null });

            migrationBuilder.UpdateData(
                table: "Operatorler",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DurumNotu", "GununBaslangicSaati", "GununBitisSaati", "MevcutDurum", "SonDurumDegisikliği" },
                values: new object[] { null, null, null, 0, null });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorDurumGecmisleri_GecisZamani",
                table: "OperatorDurumGecmisleri",
                column: "GecisZamani");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorDurumGecmisleri_IlgiliAramaLogId",
                table: "OperatorDurumGecmisleri",
                column: "IlgiliAramaLogId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorDurumGecmisleri_OperatorId",
                table: "OperatorDurumGecmisleri",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorDurumGecmisleri_OperatorId_GecisZamani",
                table: "OperatorDurumGecmisleri",
                columns: new[] { "OperatorId", "GecisZamani" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorGunlukDurumOzetleri_OperatorId",
                table: "OperatorGunlukDurumOzetleri",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorGunlukDurumOzetleri_OperatorId_Tarih",
                table: "OperatorGunlukDurumOzetleri",
                columns: new[] { "OperatorId", "Tarih" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperatorGunlukDurumOzetleri_Tarih",
                table: "OperatorGunlukDurumOzetleri",
                column: "Tarih");

            migrationBuilder.AddForeignKey(
                name: "FK_CevapSablonKullanimlar_Musteriler_MusteriId",
                table: "CevapSablonKullanimlar",
                column: "MusteriId",
                principalTable: "Musteriler",
                principalColumn: "Id"); // NoAction - SQL Server cascade path fix
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CevapSablonKullanimlar_Musteriler_MusteriId",
                table: "CevapSablonKullanimlar");

            migrationBuilder.DropTable(
                name: "OperatorDurumGecmisleri");

            migrationBuilder.DropTable(
                name: "OperatorGunlukDurumOzetleri");

            migrationBuilder.DropColumn(
                name: "DurumNotu",
                table: "Operatorler");

            migrationBuilder.DropColumn(
                name: "GununBaslangicSaati",
                table: "Operatorler");

            migrationBuilder.DropColumn(
                name: "GununBitisSaati",
                table: "Operatorler");

            migrationBuilder.DropColumn(
                name: "MevcutDurum",
                table: "Operatorler");

            migrationBuilder.DropColumn(
                name: "SonDurumDegisikliği",
                table: "Operatorler");

            migrationBuilder.AddForeignKey(
                name: "FK_CevapSablonKullanimlar_Musteriler_MusteriId",
                table: "CevapSablonKullanimlar",
                column: "MusteriId",
                principalTable: "Musteriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
