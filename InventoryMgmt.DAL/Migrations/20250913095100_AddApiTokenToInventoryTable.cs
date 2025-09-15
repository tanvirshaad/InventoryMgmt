using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddApiTokenToInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6531), new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6533) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6534), new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6535) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6536), new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6536) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6537), new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6537) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6538), new DateTime(2025, 9, 13, 9, 50, 59, 664, DateTimeKind.Utc).AddTicks(6538) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6475), new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6477) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6479), new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6479) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6480), new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6480) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6481), new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6482) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6482), new DateTime(2025, 9, 13, 9, 49, 43, 990, DateTimeKind.Utc).AddTicks(6483) });
        }
    }
}
