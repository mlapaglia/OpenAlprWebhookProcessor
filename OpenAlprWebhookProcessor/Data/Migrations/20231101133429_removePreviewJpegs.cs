using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class removePreviewJpegs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatePreviewJpeg",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "VehiclePreviewJpeg",
                table: "PlateGroups");

            migrationBuilder.Sql("VACUUM;", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlatePreviewJpeg",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclePreviewJpeg",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);
        }
    }
}
