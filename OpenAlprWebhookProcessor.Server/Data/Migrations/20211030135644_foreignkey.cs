using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class foreignkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_BestNumber_ReceivedOnEpoch",
                table: "PlateGroups",
                columns: new[] { "BestNumber", "ReceivedOnEpoch" });

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_ReceivedOnEpoch",
                table: "PlateGroups",
                column: "ReceivedOnEpoch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_BestNumber_ReceivedOnEpoch",
                table: "PlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_ReceivedOnEpoch",
                table: "PlateGroups");
        }
    }
}
