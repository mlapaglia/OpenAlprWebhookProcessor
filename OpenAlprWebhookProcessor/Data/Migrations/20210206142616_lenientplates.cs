using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class lenientplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "PlateGroups",
                newName: "BestNumber");

            migrationBuilder.AddColumn<string>(
                name: "PossibleNumbers",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PossibleNumbers",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "BestNumber",
                table: "PlateGroups",
                newName: "Number");
        }
    }
}
