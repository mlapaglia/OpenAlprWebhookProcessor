using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class zoomfocus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DayFocus",
                table: "Cameras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DayZoom",
                table: "Cameras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NightFocus",
                table: "Cameras",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NightZoom",
                table: "Cameras",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayFocus",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "DayZoom",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "NightFocus",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "NightZoom",
                table: "Cameras");
        }
    }
}
