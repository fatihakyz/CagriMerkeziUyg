using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CagriMerkeziUyg.Migrations
{
    /// <inheritdoc />
    public partial class AddKonuPropertyToMusteriAktiviteler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Konu",
                table: "MusteriAktiviteleri",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Konu",
                table: "MusteriAktiviteleri");
        }
    }
}
