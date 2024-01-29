using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class cameramask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CameraMasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CameraId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Coordinates = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraMasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraMasks_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CameraMasks_CameraId",
                table: "CameraMasks",
                column: "CameraId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraMasks");
        }
    }
}
