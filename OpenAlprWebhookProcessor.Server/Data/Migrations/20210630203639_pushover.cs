using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class pushover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PushoverAlertClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserKey = table.Column<string>(type: "TEXT", nullable: true),
                    ApiToken = table.Column<string>(type: "TEXT", nullable: true),
                    SendPlatePreview = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushoverAlertClients", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushoverAlertClients");
        }
    }
}
