using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class permissionaddedinIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Permission",
                table: "InventoryAccesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permission",
                table: "InventoryAccesses");

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
    }
}
