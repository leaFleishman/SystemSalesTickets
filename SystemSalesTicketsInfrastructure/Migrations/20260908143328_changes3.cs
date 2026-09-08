using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class changes3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EventSeats",
                columns: new[] { "EventId", "SeatId", "IsAvailable", "Version" },
                values: new object[,]
                {
                    { 1, 1, false, new Guid("11111111-1111-1111-1111-111111111111") },
                    { 1, 2, true, new Guid("22222222-2222-2222-2222-222222222222") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EventSeats",
                keyColumns: new[] { "EventId", "SeatId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "EventSeats",
                keyColumns: new[] { "EventId", "SeatId" },
                keyValues: new object[] { 1, 2 });
        }
    }
}
