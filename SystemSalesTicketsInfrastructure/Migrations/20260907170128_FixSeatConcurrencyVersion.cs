using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSeatConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.DropColumn(
      name: "Version",
      table: "Seat");

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Seat",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "Date", "Name", "NumberOfSeats", "Price" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Concert A", 2500, 15000m });

            migrationBuilder.InsertData(
                table: "Seat",
                columns: new[] { "SeatId", "IsAvailable", "Line", "Row", "Version" },
                values: new object[,]
                {
                    { 1, true, 1, 1, new Guid("3e806215-e4ca-42e8-a197-746edcb51981") },
                    { 2, true, 12, 12, new Guid("12f9a601-54a0-45f4-a5df-ca9a9143186a") }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Phone", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, "admin@example.com", "111", "0556667788", 2, "Avi" },
                    { 2, "user@example.com", "222", "0556367788", 1, "Moshe" }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "EventId", "EventName", "OrderDate", "SeatId", "UserId" },
                values: new object[] { 1, 1, "Concert A", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Events",
                keyColumn: "EventId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "SeatId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "Seat",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
