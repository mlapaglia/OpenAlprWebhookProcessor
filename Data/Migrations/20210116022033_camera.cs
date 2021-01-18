using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class camera : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Manufacturer = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenAlprName = table.Column<string>(type: "TEXT", nullable: true),
                    OpenAlprCameraId = table.Column<long>(type: "INTEGER", nullable: false),
                    CameraPassword = table.Column<string>(type: "TEXT", nullable: true),
                    CameraUsername = table.Column<string>(type: "TEXT", nullable: true),
                    UpdateOverlayTextUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cameras");
        }
    }
}
