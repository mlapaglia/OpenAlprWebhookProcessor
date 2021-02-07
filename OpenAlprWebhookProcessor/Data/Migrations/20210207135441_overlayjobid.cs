using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class overlayjobid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NextClearOverlayScheduleId",
                table: "Cameras",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextClearOverlayScheduleId",
                table: "Cameras");
        }
    }
}
