using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class moreforwardoptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ForwardGroupPreviews",
                table: "WebhookForwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ForwardGroups",
                table: "WebhookForwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ForwardSinglePlates",
                table: "WebhookForwards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForwardGroupPreviews",
                table: "WebhookForwards");

            migrationBuilder.DropColumn(
                name: "ForwardGroups",
                table: "WebhookForwards");

            migrationBuilder.DropColumn(
                name: "ForwardSinglePlates",
                table: "WebhookForwards");
        }
    }
}
