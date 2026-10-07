using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    public partial class Add100Seats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seats = new object[170, 4];

            int id = 1;

            for (int row = 1; row <= 10; row++)
            {
                for (int line = 1; line <= 17; line++)
                {
                    seats[id - 1, 0] = id;
                    seats[id - 1, 1] = line;
                    seats[id - 1, 2] = row;
                    seats[id - 1, 3] =
                        Guid.Parse($"00000000-0000-0000-0000-{id:D12}");

                    id++;
                }
            }

            migrationBuilder.InsertData(
                table: "Seat",
                columns: new[] { "Id", "Line", "Row", "Version" },
                values: seats);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int id = 170; id >= 1; id--)
            {
                migrationBuilder.DeleteData(
                    table: "Seat",
                    keyColumn: "Id",
                    keyValue: id);
            }
        }
    }
}