using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOccurredOnToProcessedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredOn",
                table: "processed_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccurredOn",
                table: "processed_events");
        }
    }
}
