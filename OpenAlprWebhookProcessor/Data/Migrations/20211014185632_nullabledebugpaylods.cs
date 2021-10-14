using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class nullabledebugpaylods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlateGroupId",
                table: "RawPlateGroups",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups",
                column: "PlateGroupId",
                principalTable: "PlateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlateGroupId",
                table: "RawPlateGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RawPlateGroups_PlateGroups_PlateGroupId",
                table: "RawPlateGroups",
                column: "PlateGroupId",
                principalTable: "PlateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
