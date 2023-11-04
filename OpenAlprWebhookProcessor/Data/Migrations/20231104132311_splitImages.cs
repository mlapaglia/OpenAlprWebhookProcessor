using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class splitImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlateImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Jpeg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    IsCompressed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateImage_PlateGroups_PlateGroupId",
                        column: x => x.PlateGroupId,
                        principalTable: "PlateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Jpeg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    IsCompressed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleImage_PlateGroups_PlateGroupId",
                        column: x => x.PlateGroupId,
                        principalTable: "PlateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlateImage_Id",
                table: "PlateImage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlateImage_PlateGroupId",
                table: "PlateImage",
                column: "PlateGroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImage_PlateGroupId",
                table: "VehicleImage",
                column: "PlateGroupId",
                unique: true);

            

            migrationBuilder.Sql(
                @"INSERT INTO PlateImage
                    (Id, PlateGroupId, Jpeg, IsCompressed)
                        SELECT lower(
                            hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || '4' || 
                            substr(hex( randomblob(2)), 2) || '-' || 
                            substr('AB89', 1 + (abs(random()) % 4) , 1)  ||
                            substr(hex(randomblob(2)), 2) || '-' || 
                            hex(randomblob(6)))
                        , Id, PlateJpeg, isPlateJpegCompressed
                        FROM PlateGroups
                        WHERE PlateJpeg IS NOT NULL;");

            migrationBuilder.Sql(
                @"INSERT INTO VehicleImage
                    (Id, PlateGroupId, Jpeg, IsCompressed)
                        SELECT lower(
                            hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || '4' || 
                            substr(hex( randomblob(2)), 2) || '-' || 
                            substr('AB89', 1 + (abs(random()) % 4) , 1)  ||
                            substr(hex(randomblob(2)), 2) || '-' || 
                            hex(randomblob(6)))
                        , Id, VehicleJpeg, isVehicleJpegCompressed
                        FROM PlateGroups
                        WHERE VehicleJpeg IS NOT NULL;");

            migrationBuilder.DropColumn(
                name: "PlateJpeg",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "VehicleJpeg",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "isPlateJpegCompressed",
                table: "PlateGroups");

            migrationBuilder.DropColumn(
                name: "isVehicleJpegCompressed",
                table: "PlateGroups");

            migrationBuilder.Sql("VACUUM;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateImage");

            migrationBuilder.DropTable(
                name: "VehicleImage");

            migrationBuilder.AddColumn<byte[]>(
                name: "PlateJpeg",
                table: "PlateGroups",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "VehicleJpeg",
                table: "PlateGroups",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isPlateJpegCompressed",
                table: "PlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isVehicleJpegCompressed",
                table: "PlateGroups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
