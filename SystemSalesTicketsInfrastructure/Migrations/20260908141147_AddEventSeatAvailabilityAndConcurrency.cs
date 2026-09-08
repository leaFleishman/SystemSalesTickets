using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSeatAvailabilityAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSeat_Events_EventId",
                table: "EventSeat");

            migrationBuilder.DropForeignKey(
                name: "FK_EventSeat_Seat_SeatId",
                table: "EventSeat");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSeat",
                table: "EventSeat");

            migrationBuilder.RenameTable(
                name: "EventSeat",
                newName: "EventSeats");

            migrationBuilder.RenameIndex(
                name: "IX_EventSeat_SeatId",
                table: "EventSeats",
                newName: "IX_EventSeats_SeatId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "EventSeats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSeats",
                table: "EventSeats",
                columns: new[] { "EventId", "SeatId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventSeats_Events_EventId",
                table: "EventSeats",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSeats_Seat_SeatId",
                table: "EventSeats",
                column: "SeatId",
                principalTable: "Seat",
                principalColumn: "SeatId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSeats_Events_EventId",
                table: "EventSeats");

            migrationBuilder.DropForeignKey(
                name: "FK_EventSeats_Seat_SeatId",
                table: "EventSeats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSeats",
                table: "EventSeats");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "EventSeats");

            migrationBuilder.RenameTable(
                name: "EventSeats",
                newName: "EventSeat");

            migrationBuilder.RenameIndex(
                name: "IX_EventSeats_SeatId",
                table: "EventSeat",
                newName: "IX_EventSeat_SeatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSeat",
                table: "EventSeat",
                columns: new[] { "EventId", "SeatId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EventSeat_Events_EventId",
                table: "EventSeat",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventSeat_Seat_SeatId",
                table: "EventSeat",
                column: "SeatId",
                principalTable: "Seat",
                principalColumn: "SeatId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
