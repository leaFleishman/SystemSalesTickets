using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderSeatUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EventId_Id",
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

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId_Id",
                table: "Orders",
                columns: new[] { "EventId", "Id" },
                unique: true);
        }
    }


}
