using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropColumn(
                name: "ReceivedOn",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "PlateNumber",
                table: "PlateGroups",
                newName: "VehicleDescription");

            migrationBuilder.RenameColumn(
                name: "PlateJpeg",
                table: "PlateGroups",
                newName: "OpenAlprUuid");

            migrationBuilder.RenameColumn(
                name: "PlateConfidence",
                table: "PlateGroups",
                newName: "Direction");

            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "PlateGroups",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Jpeg",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "PlateGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReceivedOnEpoch",
                table: "PlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "Jpeg",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "ReceivedOnEpoch",
                table: "PlateGroups");

            migrationBuilder.RenameColumn(
                name: "VehicleDescription",
                table: "PlateGroups",
                newName: "PlateNumber");

            migrationBuilder.RenameColumn(
                name: "OpenAlprUuid",
                table: "PlateGroups",
                newName: "PlateJpeg");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "PlateGroups",
                newName: "PlateConfidence");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReceivedOn",
                table: "PlateGroups",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyId = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });
        }
    }
}
