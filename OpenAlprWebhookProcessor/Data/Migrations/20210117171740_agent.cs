using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class agent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndpointUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Hostname = table.Column<string>(type: "TEXT", nullable: true),
                    Uid = table.Column<string>(type: "TEXT", nullable: true),
                    OpenAlprWebServerApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    OpenAlprWebServerUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");
        }
    }
}
