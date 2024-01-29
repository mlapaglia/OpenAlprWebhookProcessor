using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class allenabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendEveryPlateEnabled",
                table: "WebPushSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendEveryPlateEnabled",
                table: "PushoverAlertClients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendEveryPlateEnabled",
                table: "WebPushSettings");

            migrationBuilder.DropColumn(
                name: "SendEveryPlateEnabled",
                table: "PushoverAlertClients");
        }
    }
}
