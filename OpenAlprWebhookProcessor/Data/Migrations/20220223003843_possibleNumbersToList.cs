using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    public partial class possibleNumbersToList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlateGroupPossibleNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlateGroupPossibleNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlateGroupPossibleNumbers_PlateGroups_PlateGroupId",
                        column: x => x.PlateGroupId,
                        principalTable: "PlateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO PlateGroupPossibleNumbers
	                WITH split(id, number, str) AS(
		                SELECT id, '', PossibleNumbers || ',' FROM PlateGroups
		                UNION ALL SELECT id,
		                substr(str, 0, instr(str, ',')),
		                substr(str, instr(str, ',') + 1)
		                FROM split WHERE str)
	                SELECT
		                hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || substr(hex(randomblob(2)), 2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)), 2) || '-' || hex(randomblob(6)) as Id,
		                id as PlateGroupId,
		                number as Number
	                FROM split 
	                WHERE number
	                ORDER BY id;", true);

            migrationBuilder.CreateIndex(
                name: "IX_PlateGroupPossibleNumbers_PlateGroupId",
                table: "PlateGroupPossibleNumbers",
                column: "PlateGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlateGroupPossibleNumbers");
        }
    }
}
