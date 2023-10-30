using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class possiblenumberindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlateGroupPossibleNumbers_PlateGroupId",
                table: "PlateGroupPossibleNumbers");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroupPossibleNumbers_PlateGroupId_Number",
                table: "PlateGroupPossibleNumbers",
                columns: new[] { "PlateGroupId", "Number" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlateGroupPossibleNumbers_PlateGroupId_Number",
                table: "PlateGroupPossibleNumbers");

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroupPossibleNumbers_PlateGroupId",
                table: "PlateGroupPossibleNumbers",
                column: "PlateGroupId");
        }
    }
}
