using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddApiTokenToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Inventories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Inventories");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3128), new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3130) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3134), new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3134) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3135), new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3136) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3137), new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3137) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3138), new DateTime(2025, 8, 31, 10, 44, 58, 790, DateTimeKind.Utc).AddTicks(3138) });
        }
    }
}
