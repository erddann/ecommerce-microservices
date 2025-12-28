using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<JsonElement>(
                name: "Data",
                table: "OutboxMessages",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "TemplateCode", "Channel", "Language", "Subject", "Body", "IsActive", "Version" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440000"), "ORDER_CONFIRMED", 1, "en", "Order Confirmation", "Dear Customer,\n\nYour order {OrderNumber} has been confirmed successfully.\n\nOrder Total: {OrderTotal}\n\nThank you for shopping with us!", true, 1 },
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), "ORDER_CANCELLED", 1, "en", "Order Cancellation", "Dear Customer,\n\nWe regret to inform you that your order {OrderId} has been cancelled.\n\nReason: {Reason}\n\nPlease contact support if you have any questions.", true, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("550e8400-e29b-41d4-a716-446655440000"),
                    new Guid("550e8400-e29b-41d4-a716-446655440001")
                });

            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "OutboxMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(JsonElement),
                oldType: "jsonb");
        }
    }
}
