using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "KayitTarihi", "SonGuncelleme" },
                values: new object[] { new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "KayitTarihi", "SonGuncelleme" },
                values: new object[] { new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 5, 10, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "KayitTarihi", "SonGuncelleme" },
                values: new object[] { new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5101), new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5355) });

            migrationBuilder.UpdateData(
                table: "Musteriler",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "KayitTarihi", "SonGuncelleme" },
                values: new object[] { new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5874), new DateTime(2025, 8, 5, 15, 58, 59, 811, DateTimeKind.Local).AddTicks(5874) });
        }
    }
}
