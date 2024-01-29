using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAlprWebhookProcessor.Migrations
{
    /// <inheritdoc />
    public partial class pushnotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptionKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MobilePushSubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptionKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushSubscriptionKeys_PushSubscriptions_MobilePushSubscriptionId",
                        column: x => x.MobilePushSubscriptionId,
                        principalTable: "PushSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptionKeys_MobilePushSubscriptionId",
                table: "PushSubscriptionKeys",
                column: "MobilePushSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushSubscriptionKeys");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");
        }
    }
}
