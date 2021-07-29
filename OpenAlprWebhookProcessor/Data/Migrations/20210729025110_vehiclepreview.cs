using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class vehiclepreview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Jpeg",
                table: "PlateGroups",
                newName: "VehiclePreviewJpeg");

            migrationBuilder.AddColumn<string>(
                name: "PlatePreviewJpeg",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatePreviewJpeg",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "VehiclePreviewJpeg",
                table: "PlateGroups",
                newName: "Jpeg");
        }
    }
}
