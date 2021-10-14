using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class rawplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    PlateGroupId = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivedOnEpoch = table.Column<long>(type: "INTEGER", nullable: false),
                    RawPlateGroup = table.Column<string>(type: "TEXT", nullable: true),
                    WasProcessedCorrectly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawPlateGroups", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawPlateGroups");

            migrationBuilder.DropColumn(
                name: "IsDebugEnabled",
                table: "Agents");
        }
    }
}
