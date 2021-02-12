using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class timezone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TimezoneOffset",
                table: "Cameras",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TimeZoneOffset",
                table: "Agents",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimezoneOffset",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "TimeZoneOffset",
                table: "Agents");
        }
    }
}
