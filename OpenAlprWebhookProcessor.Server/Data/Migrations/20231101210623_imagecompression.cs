using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class imagecompression : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPlateJpegCompressed",
                table: "PlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isVehicleJpegCompressed",
                table: "PlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImageCompressionEnabled",
                table: "Agents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isPlateJpegCompressed",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "isVehicleJpegCompressed",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "IsImageCompressionEnabled",
                table: "Agents");
        }
    }
}
