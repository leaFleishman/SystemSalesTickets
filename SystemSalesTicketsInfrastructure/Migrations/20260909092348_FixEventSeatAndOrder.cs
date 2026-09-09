using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEventSeatAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EventId_SeatId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "SeatId",
                table: "Seat",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "Orders",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "Events",
                newName: "Id");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                column: "SeatId",
                value: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId_Id",
                table: "Orders",
                columns: new[] { "EventId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_EventId_Id",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Seat",
                newName: "SeatId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Orders",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Events",
                newName: "EventId");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1,
                column: "SeatId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_EventId_SeatId",
                table: "Orders",
                columns: new[] { "EventId", "SeatId" },
                unique: true);
        }
    }
}
