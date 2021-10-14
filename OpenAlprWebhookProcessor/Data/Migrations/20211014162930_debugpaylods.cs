using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class debugpaylods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RawPlateGroupId",
                table: "PlateGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDebugEnabled",
                table: "Agents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RawPlateGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RawPlateGroup = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawPlateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                        column: x => x.PlateGroupId,
                        principalTable: "PlateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawPlateGroups_PlateGroupId",
                table: "RawPlateGroups",
                column: "PlateGroupId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawPlateGroups");

            migrationBuilder.DropColumn(
                name: "RawPlateGroupId",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "IsDebugEnabled",
                table: "Agents");
        }
    }
}
