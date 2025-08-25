using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedNumericFieldConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NumericField1IsInteger",
                table: "Inventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField1MaxValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField1MinValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField1StepValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.01m);

            migrationBuilder.AddColumn<string>(
                name: "NumericField1DisplayFormat",
                table: "Inventories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NumericField2IsInteger",
                table: "Inventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField2MaxValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField2MinValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField2StepValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.01m);

            migrationBuilder.AddColumn<string>(
                name: "NumericField2DisplayFormat",
                table: "Inventories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NumericField3IsInteger",
                table: "Inventories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField3MaxValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField3MinValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NumericField3StepValue",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.01m);

            migrationBuilder.AddColumn<string>(
                name: "NumericField3DisplayFormat",
                table: "Inventories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericField1IsInteger",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField1MaxValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField1MinValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField1StepValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField1DisplayFormat",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField2IsInteger",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField2MaxValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField2MinValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField2StepValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField2DisplayFormat",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField3IsInteger",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField3MaxValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField3MinValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField3StepValue",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "NumericField3DisplayFormat",
                table: "Inventories");
        }
    }
}
