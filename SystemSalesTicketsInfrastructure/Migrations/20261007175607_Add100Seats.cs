using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemSalesTickets.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add100Seats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 20.0);

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 1,
                column: "Version",
                value: new Guid("00000001-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Line", "Row", "Version" },
                values: new object[] { 2, 1, new Guid("00000002-0000-0000-0000-000000000000") });

            migrationBuilder.InsertData(
                table: "Seat",
                columns: new[] { "Id", "Line", "Row", "Version" },
                values: new object[,]
                {
                    { 3, 3, 1, new Guid("00000003-0000-0000-0000-000000000000") },
                    { 4, 4, 1, new Guid("00000004-0000-0000-0000-000000000000") },
                    { 5, 5, 1, new Guid("00000005-0000-0000-0000-000000000000") },
                    { 6, 6, 1, new Guid("00000006-0000-0000-0000-000000000000") },
                    { 7, 7, 1, new Guid("00000007-0000-0000-0000-000000000000") },
                    { 8, 8, 1, new Guid("00000008-0000-0000-0000-000000000000") },
                    { 9, 9, 1, new Guid("00000009-0000-0000-0000-000000000000") },
                    { 10, 10, 1, new Guid("00000010-0000-0000-0000-000000000000") },
                    { 11, 1, 2, new Guid("00000011-0000-0000-0000-000000000000") },
                    { 12, 2, 2, new Guid("00000012-0000-0000-0000-000000000000") },
                    { 13, 3, 2, new Guid("00000013-0000-0000-0000-000000000000") },
                    { 14, 4, 2, new Guid("00000014-0000-0000-0000-000000000000") },
                    { 15, 5, 2, new Guid("00000015-0000-0000-0000-000000000000") },
                    { 16, 6, 2, new Guid("00000016-0000-0000-0000-000000000000") },
                    { 17, 7, 2, new Guid("00000017-0000-0000-0000-000000000000") },
                    { 18, 8, 2, new Guid("00000018-0000-0000-0000-000000000000") },
                    { 19, 9, 2, new Guid("00000019-0000-0000-0000-000000000000") },
                    { 20, 10, 2, new Guid("00000020-0000-0000-0000-000000000000") },
                    { 21, 1, 3, new Guid("00000021-0000-0000-0000-000000000000") },
                    { 22, 2, 3, new Guid("00000022-0000-0000-0000-000000000000") },
                    { 23, 3, 3, new Guid("00000023-0000-0000-0000-000000000000") },
                    { 24, 4, 3, new Guid("00000024-0000-0000-0000-000000000000") },
                    { 25, 5, 3, new Guid("00000025-0000-0000-0000-000000000000") },
                    { 26, 6, 3, new Guid("00000026-0000-0000-0000-000000000000") },
                    { 27, 7, 3, new Guid("00000027-0000-0000-0000-000000000000") },
                    { 28, 8, 3, new Guid("00000028-0000-0000-0000-000000000000") },
                    { 29, 9, 3, new Guid("00000029-0000-0000-0000-000000000000") },
                    { 30, 10, 3, new Guid("00000030-0000-0000-0000-000000000000") },
                    { 31, 1, 4, new Guid("00000031-0000-0000-0000-000000000000") },
                    { 32, 2, 4, new Guid("00000032-0000-0000-0000-000000000000") },
                    { 33, 3, 4, new Guid("00000033-0000-0000-0000-000000000000") },
                    { 34, 4, 4, new Guid("00000034-0000-0000-0000-000000000000") },
                    { 35, 5, 4, new Guid("00000035-0000-0000-0000-000000000000") },
                    { 36, 6, 4, new Guid("00000036-0000-0000-0000-000000000000") },
                    { 37, 7, 4, new Guid("00000037-0000-0000-0000-000000000000") },
                    { 38, 8, 4, new Guid("00000038-0000-0000-0000-000000000000") },
                    { 39, 9, 4, new Guid("00000039-0000-0000-0000-000000000000") },
                    { 40, 10, 4, new Guid("00000040-0000-0000-0000-000000000000") },
                    { 41, 1, 5, new Guid("00000041-0000-0000-0000-000000000000") },
                    { 42, 2, 5, new Guid("00000042-0000-0000-0000-000000000000") },
                    { 43, 3, 5, new Guid("00000043-0000-0000-0000-000000000000") },
                    { 44, 4, 5, new Guid("00000044-0000-0000-0000-000000000000") },
                    { 45, 5, 5, new Guid("00000045-0000-0000-0000-000000000000") },
                    { 46, 6, 5, new Guid("00000046-0000-0000-0000-000000000000") },
                    { 47, 7, 5, new Guid("00000047-0000-0000-0000-000000000000") },
                    { 48, 8, 5, new Guid("00000048-0000-0000-0000-000000000000") },
                    { 49, 9, 5, new Guid("00000049-0000-0000-0000-000000000000") },
                    { 50, 10, 5, new Guid("00000050-0000-0000-0000-000000000000") },
                    { 51, 1, 6, new Guid("00000051-0000-0000-0000-000000000000") },
                    { 52, 2, 6, new Guid("00000052-0000-0000-0000-000000000000") },
                    { 53, 3, 6, new Guid("00000053-0000-0000-0000-000000000000") },
                    { 54, 4, 6, new Guid("00000054-0000-0000-0000-000000000000") },
                    { 55, 5, 6, new Guid("00000055-0000-0000-0000-000000000000") },
                    { 56, 6, 6, new Guid("00000056-0000-0000-0000-000000000000") },
                    { 57, 7, 6, new Guid("00000057-0000-0000-0000-000000000000") },
                    { 58, 8, 6, new Guid("00000058-0000-0000-0000-000000000000") },
                    { 59, 9, 6, new Guid("00000059-0000-0000-0000-000000000000") },
                    { 60, 10, 6, new Guid("00000060-0000-0000-0000-000000000000") },
                    { 61, 1, 7, new Guid("00000061-0000-0000-0000-000000000000") },
                    { 62, 2, 7, new Guid("00000062-0000-0000-0000-000000000000") },
                    { 63, 3, 7, new Guid("00000063-0000-0000-0000-000000000000") },
                    { 64, 4, 7, new Guid("00000064-0000-0000-0000-000000000000") },
                    { 65, 5, 7, new Guid("00000065-0000-0000-0000-000000000000") },
                    { 66, 6, 7, new Guid("00000066-0000-0000-0000-000000000000") },
                    { 67, 7, 7, new Guid("00000067-0000-0000-0000-000000000000") },
                    { 68, 8, 7, new Guid("00000068-0000-0000-0000-000000000000") },
                    { 69, 9, 7, new Guid("00000069-0000-0000-0000-000000000000") },
                    { 70, 10, 7, new Guid("00000070-0000-0000-0000-000000000000") },
                    { 71, 1, 8, new Guid("00000071-0000-0000-0000-000000000000") },
                    { 72, 2, 8, new Guid("00000072-0000-0000-0000-000000000000") },
                    { 73, 3, 8, new Guid("00000073-0000-0000-0000-000000000000") },
                    { 74, 4, 8, new Guid("00000074-0000-0000-0000-000000000000") },
                    { 75, 5, 8, new Guid("00000075-0000-0000-0000-000000000000") },
                    { 76, 6, 8, new Guid("00000076-0000-0000-0000-000000000000") },
                    { 77, 7, 8, new Guid("00000077-0000-0000-0000-000000000000") },
                    { 78, 8, 8, new Guid("00000078-0000-0000-0000-000000000000") },
                    { 79, 9, 8, new Guid("00000079-0000-0000-0000-000000000000") },
                    { 80, 10, 8, new Guid("00000080-0000-0000-0000-000000000000") },
                    { 81, 1, 9, new Guid("00000081-0000-0000-0000-000000000000") },
                    { 82, 2, 9, new Guid("00000082-0000-0000-0000-000000000000") },
                    { 83, 3, 9, new Guid("00000083-0000-0000-0000-000000000000") },
                    { 84, 4, 9, new Guid("00000084-0000-0000-0000-000000000000") },
                    { 85, 5, 9, new Guid("00000085-0000-0000-0000-000000000000") },
                    { 86, 6, 9, new Guid("00000086-0000-0000-0000-000000000000") },
                    { 87, 7, 9, new Guid("00000087-0000-0000-0000-000000000000") },
                    { 88, 8, 9, new Guid("00000088-0000-0000-0000-000000000000") },
                    { 89, 9, 9, new Guid("00000089-0000-0000-0000-000000000000") },
                    { 90, 10, 9, new Guid("00000090-0000-0000-0000-000000000000") },
                    { 91, 1, 10, new Guid("00000091-0000-0000-0000-000000000000") },
                    { 92, 2, 10, new Guid("00000092-0000-0000-0000-000000000000") },
                    { 93, 3, 10, new Guid("00000093-0000-0000-0000-000000000000") },
                    { 94, 4, 10, new Guid("00000094-0000-0000-0000-000000000000") },
                    { 95, 5, 10, new Guid("00000095-0000-0000-0000-000000000000") },
                    { 96, 6, 10, new Guid("00000096-0000-0000-0000-000000000000") },
                    { 97, 7, 10, new Guid("00000097-0000-0000-0000-000000000000") },
                    { 98, 8, 10, new Guid("00000098-0000-0000-0000-000000000000") },
                    { 99, 9, 10, new Guid("00000099-0000-0000-0000-000000000000") },
                    { 100, 10, 10, new Guid("00000100-0000-0000-0000-000000000000") }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "15000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 73);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 74);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 75);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 76);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 77);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 78);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 79);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 80);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 82);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 83);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 84);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 85);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 86);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 87);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 88);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 89);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 96);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 97);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 98);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 99);

            migrationBuilder.DeleteData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 15000.0);

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 1,
                column: "Version",
                value: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.UpdateData(
                table: "Seat",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Line", "Row", "Version" },
                values: new object[] { 12, 12, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "admin@example.com");
        }
    }
}
