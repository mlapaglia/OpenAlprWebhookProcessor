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
                newName: "PossibleNumbers");

            migrationBuilder.AddColumn<string>(
                name: "BestNumber",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestNumber",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "PossibleNumbers",
                table: "PlateGroups",
                newName: "Number");
        }
    }
}
