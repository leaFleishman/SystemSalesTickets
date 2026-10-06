using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemSalesTickets.Data.Migrations
{
    public partial class AddUniqueSeatRowLine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Seat_Row_Line",
                table: "Seat",
                columns: new[] { "Row", "Line" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seat_Row_Line",
                table: "Seat");
        }
    }
}