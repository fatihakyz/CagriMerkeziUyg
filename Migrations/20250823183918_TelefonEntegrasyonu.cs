using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class TelefonEntegrasyonu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AramaLoglari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TelefonNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MusteriId = table.Column<int>(type: "int", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    BaslangicZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisZamani = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CagriSuresi = table.Column<int>(type: "int", nullable: true),
                    Notlar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AramaLoglari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AramaLoglari_Musteriler_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Musteriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AramaLoglari_Operatorler_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operatorler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AramaLoglari_BaslangicZamani",
                table: "AramaLoglari",
                column: "BaslangicZamani");

            migrationBuilder.CreateIndex(
                name: "IX_AramaLoglari_MusteriId",
                table: "AramaLoglari",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_AramaLoglari_OperatorId",
                table: "AramaLoglari",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AramaLoglari_TelefonNo",
                table: "AramaLoglari",
                column: "TelefonNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AramaLoglari");
        }
    }
}
