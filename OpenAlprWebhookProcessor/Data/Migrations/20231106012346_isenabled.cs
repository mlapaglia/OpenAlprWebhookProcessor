using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class isenabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "WebPushSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "WebPushSettings");
        }
    }
}
