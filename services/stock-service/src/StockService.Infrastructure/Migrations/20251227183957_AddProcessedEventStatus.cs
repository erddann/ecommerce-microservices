using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedEventStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "processed_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "processed_events");
        }
    }
}
