using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class enricherEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RunAlways",
                table: "Enrichers");

            migrationBuilder.DropColumn(
                name: "RunAtNight",
                table: "Enrichers");

            migrationBuilder.RenameColumn(
                name: "RunManually",
                table: "Enrichers",
                newName: "EnrichmentType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnrichmentType",
                table: "Enrichers",
                newName: "RunManually");

            migrationBuilder.AddColumn<bool>(
                name: "RunAlways",
                table: "Enrichers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RunAtNight",
                table: "Enrichers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
