using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class uuidindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlateGroups_OpenAlprUuid",
                table: "PlateGroups",
                column: "OpenAlprUuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlateGroups_OpenAlprUuid",
                table: "PlateGroups");
        }
    }
}
