using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class usertablealtered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6583), new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6585) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6587), new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6587) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6588), new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6588) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6589), new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6590) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6591), new DateTime(2025, 8, 29, 6, 35, 38, 735, DateTimeKind.Utc).AddTicks(6591) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(207), new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(209) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(210), new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(210) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(211), new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(212) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(213), new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(213) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(214), new DateTime(2025, 8, 29, 6, 29, 29, 898, DateTimeKind.Utc).AddTicks(214) });
        }
    }
}
