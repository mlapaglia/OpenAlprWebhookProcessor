using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlateGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OpenAlprCameraId = table.Column<int>(nullable: false),
                    OpenAlprProcessingTimeMs = table.Column<double>(nullable: false),
                    IsAlert = table.Column<bool>(nullable: false),
                    AlertDescription = table.Column<string>(nullable: true),
                    ReceivedOn = table.Column<DateTimeOffset>(nullable: false),
                    PlateNumber = table.Column<string>(nullable: true),
                    PlateJpeg = table.Column<string>(nullable: true),
                    PlateConfidence = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateGroups", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateGroups");
        }
    }
}
