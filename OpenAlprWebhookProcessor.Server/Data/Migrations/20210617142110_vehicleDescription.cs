using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class vehicleDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleDescription",
                table: "PlateGroups",
                newName: "VehicleYear");

            migrationBuilder.AddColumn<string>(
                name: "VehicleColor",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleMake",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleMakeModel",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleType",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VehicleColor",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "VehicleMake",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "VehicleMakeModel",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "VehicleYear",
                table: "PlateGroups",
                newName: "VehicleDescription");
        }
    }
}
