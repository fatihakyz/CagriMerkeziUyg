using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class MusteriEtiketlemeSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MusteriTipi",
                table: "Musteriler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OzelNotlar",
                table: "Musteriler",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MusteriEtiketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Etiket adı"),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Etiket açıklaması"),
                    RenkKodu = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false, defaultValue: "#007bff", comment: "Etiket renk kodu"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Etiket aktif durumu"),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Etiket oluşturulma tarihi")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriEtiketleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusteriEtiketAtamalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriId = table.Column<int>(type: "int", nullable: false),
                    MusteriEtiketiId = table.Column<int>(type: "int", nullable: false),
                    AtamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Etiket atama tarihi"),
                    AtayanOperatorId = table.Column<int>(type: "int", nullable: true),
                    Notlar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Atama notları")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusteriEtiketAtamalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusteriEtiketAtamalari_MusteriEtiketleri_MusteriEtiketiId",
                        column: x => x.MusteriEtiketiId,
                        principalTable: "MusteriEtiketleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusteriEtiketAtamalari_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MusteriEtiketAtamalari_Operatorler_AtayanOperatorId",
                        column: x => x.AtayanOperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "MusteriEtiketleri",
                columns: new[] { "Id", "Aciklama", "Ad", "Aktif", "OlusturulmaTarihi", "RenkKodu" },
                values: new object[,]
                {
                    { 1, "Yüksek değerli müşteriler", "VIP Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#FFD700" },
                    { 2, "Düzenli olarak hizmet alan müşteriler", "Düzenli Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#28a745" },
                    { 3, "Yeni kayıt olan müşteriler", "Yeni Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#17a2b8" },
                    { 4, "Dikkatli yaklaşılması gereken müşteriler", "Riskli Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#dc3545" },
                    { 5, "Sık sık şikayet eden müşteriler", "Şikayetçi Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#fd7e14" },
                    { 6, "Hizmetlerden memnun olan müşteriler", "Memnun Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#6f42c1" },
                    { 7, "Kurumsal müşteriler", "Kurumsal Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#20c997" },
                    { 8, "Bireysel müşteriler", "Bireysel Müşteri", true, new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "#6c757d" }
                });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MusteriTipi", "OzelNotlar" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MusteriTipi", "OzelNotlar" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketAtamalari_AtamaTarihi",
                table: "MusteriEtiketAtamalari",
                column: "AtamaTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketAtamalari_AtayanOperatorId",
                table: "MusteriEtiketAtamalari",
                column: "AtayanOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketAtamalari_MusteriEtiketiId",
                table: "MusteriEtiketAtamalari",
                column: "MusteriEtiketiId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketAtamalari_MusteriId",
                table: "MusteriEtiketAtamalari",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketAtamalari_MusteriId_MusteriEtiketiId",
                table: "MusteriEtiketAtamalari",
                columns: new[] { "MusteriId", "MusteriEtiketiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusteriEtiketleri_Ad",
                table: "MusteriEtiketleri",
                column: "Ad",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusteriEtiketAtamalari");

            migrationBuilder.DropTable(
                name: "MusteriEtiketleri");

            migrationBuilder.DropColumn(
                name: "MusteriTipi",
                table: "Musteriler");

            migrationBuilder.DropColumn(
                name: "OzelNotlar",
                table: "Musteriler");
        }
    }
}
