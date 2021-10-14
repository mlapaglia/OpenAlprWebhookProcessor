using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class nvm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups");

            migrationBuilder.DropIndex(
                name: "IX_RawPlateGroups_PlateGroupId",
                table: "RawPlateGroups");

            migrationBuilder.DropColumn(
                name: "RawPlateGroupId",
                table: "PlateGroups");

            migrationBuilder.AddColumn<long>(
                name: "ReceivedOnEpoch",
                table: "RawPlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "WasProcessedCorrectly",
                table: "RawPlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedOnEpoch",
                table: "RawPlateGroups");

            migrationBuilder.DropColumn(
                name: "WasProcessedCorrectly",
                table: "RawPlateGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "RawPlateGroupId",
                table: "PlateGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RawPlateGroups_PlateGroupId",
                table: "RawPlateGroups",
                column: "PlateGroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups",
                column: "PlateGroupId",
                principalTable: "PlateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
