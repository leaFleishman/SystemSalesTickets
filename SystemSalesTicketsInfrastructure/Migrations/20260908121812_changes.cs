using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Seat");

            migrationBuilder.CreateTable(
                name: "EventSeat",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    SeatId = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSeat", x => new { x.EventId, x.SeatId });
                    table.ForeignKey(
                        name: "FK_EventSeat_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSeat_Seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat",
                        principalColumn: "SeatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 1,
                column: "Version",
                value: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 2,
                column: "Version",
                value: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.CreateIndex(
                name: "IX_EventSeat_SeatId",
                table: "EventSeat",
                column: "SeatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSeat");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Seat",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 1,
                columns: new[] { "IsAvailable", "Version" },
                values: new object[] { false, new Guid("16bec816-ec0f-4e7a-aeed-5bb3cd116f50") });

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 2,
                columns: new[] { "IsAvailable", "Version" },
                values: new object[] { false, new Guid("ce287448-1e30-4e0c-a6e0-a669a9efe4e6") });
        }
    }
}
