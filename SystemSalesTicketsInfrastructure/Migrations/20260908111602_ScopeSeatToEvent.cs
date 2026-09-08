using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScopeSeatToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EventId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId_SeatId",
                table: "Orders",
                columns: new[] { "EventId", "SeatId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EventId_SeatId",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 1,
                column: "Version",
                value: new Guid("3e806215-e4ca-42e8-a197-746edcb51981"));

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 2,
                column: "Version",
                value: new Guid("12f9a601-54a0-45f4-a5df-ca9a9143186a"));

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId",
                table: "Orders",
                column: "EventId");
        }
    }
}
