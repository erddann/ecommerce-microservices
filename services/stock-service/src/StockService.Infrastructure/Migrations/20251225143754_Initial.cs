using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QueueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "stocks",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocks", x => x.ProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_EventType_EventId",
                table: "processed_events",
                columns: new[] { "EventType", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_QueueName_ProcessedOn",
                table: "processed_events",
                columns: new[] { "QueueName", "ProcessedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "stocks");
        }
    }
}
