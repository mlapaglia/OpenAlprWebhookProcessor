using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class imageuuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SampleImageUrl",
                table: "Cameras",
                newName: "LatestProcessedPlateUuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatestProcessedPlateUuid",
                table: "Cameras",
                newName: "SampleImageUrl");
        }
    }
}
