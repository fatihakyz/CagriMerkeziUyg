using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class RandevuSistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Randevu başlığı"),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Randevu açıklaması"),
                    RandevuZamani = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Randevu tarihi ve saati"),
                    BitisZamani = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Randevu bitiş zamanı"),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Randevu tipi"),
                    Durum = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Bekliyor", comment: "Randevu durumu"),
                    Oncelik = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Normal", comment: "Randevu önceliği"),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    OlusturanOperatorId = table.Column<int>(type: "int", nullable: true),
                    HatirlatmaAktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Hatırlatma aktif mi"),
                    HatirlatmaSuresi = table.Column<int>(type: "int", nullable: false, defaultValue: 15, comment: "Hatırlatma süresi (dakika)"),
                    HatirlatmaGonderildi = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Hatırlatma gönderildi mi"),
                    TamamlanmaNotu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Tamamlanma notu"),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Randevu oluşturulma tarihi"),
                    SonGuncelleme = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Son güncelleme tarihi")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Randevular_Operatorler_OlusturanOperatorId",
                        column: x => x.OlusturanOperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Randevular_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_Durum",
                table: "Randevular",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_MusteriId",
                table: "Randevular",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_OlusturanOperatorId",
                table: "Randevular",
                column: "OlusturanOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_OperatorId",
                table: "Randevular",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_RandevuZamani",
                table: "Randevular",
                column: "RandevuZamani");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_RandevuZamani_OperatorId",
                table: "Randevular",
                columns: new[] { "RandevuZamani", "OperatorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Randevular");
        }
    }
}
