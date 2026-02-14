using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class AramaTipiEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CagriSuresi",
                table: "AramaLoglari",
                newName: "MusteriMemnuniyet");

            migrationBuilder.AddColumn<double>(
                name: "Sure",
                table: "AramaLoglari",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tip",
                table: "AramaLoglari",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sure",
                table: "AramaLoglari");

            migrationBuilder.DropColumn(
                name: "Tip",
                table: "AramaLoglari");

            migrationBuilder.RenameColumn(
                name: "MusteriMemnuniyet",
                table: "AramaLoglari",
                newName: "CagriSuresi");
        }
    }
}
