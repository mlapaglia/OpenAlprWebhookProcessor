using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class filterindexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleColor",
                table: "PlateGroups",
                column: "VehicleColor");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleMake",
                table: "PlateGroups",
                column: "VehicleMake");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleMakeModel",
                table: "PlateGroups",
                column: "VehicleMakeModel");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleRegion",
                table: "PlateGroups",
                column: "VehicleRegion");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleType",
                table: "PlateGroups",
                column: "VehicleType");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_VehicleYear",
                table: "PlateGroups",
                column: "VehicleYear");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleColor",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleMake",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleMakeModel",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleRegion",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleType",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_VehicleYear",
                table: "PlateGroups");
        }
    }
}
