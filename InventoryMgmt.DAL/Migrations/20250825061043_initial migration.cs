using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreferredTheme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CustomIdFormat = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CustomIdElements = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TextField1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TextField1Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TextField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    TextField2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TextField2Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TextField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    TextField3Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TextField3Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TextField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultiTextField1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MultiTextField1Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MultiTextField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultiTextField2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MultiTextField2Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MultiTextField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultiTextField3Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MultiTextField3Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MultiTextField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumericField1Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumericField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumericField2Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumericField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField3Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumericField3Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumericField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentField1Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentField2Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField3Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentField3Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField1Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BooleanField1Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BooleanField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField2Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BooleanField2Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BooleanField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField3Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BooleanField3Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BooleanField3ShowInTable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventories_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAccesses",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAccesses", x => new { x.InventoryId, x.UserId });
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTags",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTags", x => new { x.InventoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_InventoryTags_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    CustomId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    LikesCount = table.Column<int>(type: "int", nullable: false),
                    TextField1Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TextField2Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TextField3Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MultiTextField1Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MultiTextField2Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MultiTextField3Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NumericField1Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NumericField2Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NumericField3Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DocumentField1Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentField2Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DocumentField3Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BooleanField1Value = table.Column<bool>(type: "bit", nullable: true),
                    BooleanField2Value = table.Column<bool>(type: "bit", nullable: true),
                    BooleanField3Value = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemLikes",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLikes", x => new { x.ItemId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ItemLikes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9634), "Office equipment and devices", "Equipment", new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9637) },
                    { 2, new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9638), "Office furniture and fixtures", "Furniture", new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9639) },
                    { 3, new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9640), "Books and publications", "Books", new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9640) },
                    { 4, new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9641), "Important documents and records", "Documents", new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9642) },
                    { 5, new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9643), "Other miscellaneous items", "Other", new DateTime(2025, 8, 25, 6, 10, 43, 184, DateTimeKind.Utc).AddTicks(9643) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InventoryId",
                table: "Comments",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId",
                table: "InventoryAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTags_TagId",
                table: "InventoryTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLikes_UserId",
                table: "ItemLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Inventory_CustomId",
                table: "Items",
                columns: new[] { "InventoryId", "CustomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedById",
                table: "Items",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "InventoryAccesses");

            migrationBuilder.DropTable(
                name: "InventoryTags");

            migrationBuilder.DropTable(
                name: "ItemLikes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
