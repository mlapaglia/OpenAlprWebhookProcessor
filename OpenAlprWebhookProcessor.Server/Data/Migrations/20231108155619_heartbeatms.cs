using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class heartbeatms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastHeartbeatEpochMs",
                table: "Agents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHeartbeatEpochMs",
                table: "Agents");
        }
    }
}
