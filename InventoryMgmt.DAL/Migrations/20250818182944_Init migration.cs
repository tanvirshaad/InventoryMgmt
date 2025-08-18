using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryMgmt.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Initmigration : Migration
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
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredTheme = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedById1 = table.Column<int>(type: "int", nullable: false)
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
                        name: "FK_Inventories_User_CreatedById1",
                        column: x => x.CreatedById1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById1 = table.Column<int>(type: "int", nullable: false)
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
                        name: "FK_Comments_User_CreatedById1",
                        column: x => x.CreatedById1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCustomIdFormats",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    FormatJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCustomIdFormats", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_InventoryCustomIdFormats_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryFieldConfigurations",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    TextField1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField1Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    TextField2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField2Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    TextField3Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField3Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultilineTextField1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField1Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultilineTextField2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField2Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    MultilineTextField3Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField3Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField1Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField2Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    NumericField3Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField3Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField1Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField2Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    DocumentField3Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField3Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField1Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField1Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField1ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField2Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField2Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField2ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    BooleanField3Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField3Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField3ShowInTable = table.Column<bool>(type: "bit", nullable: false),
                    FieldOrderJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryFieldConfigurations", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_InventoryFieldConfigurations_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
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
                    CustomId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextField1Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField2Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TextField3Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField1Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField2Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultilineTextField3Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumericField1Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    NumericField2Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    NumericField3Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DocumentField1Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField2Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentField3Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BooleanField1Value = table.Column<bool>(type: "bit", nullable: true),
                    BooleanField2Value = table.Column<bool>(type: "bit", nullable: true),
                    BooleanField3Value = table.Column<bool>(type: "bit", nullable: true),
                    CreatedById1 = table.Column<int>(type: "int", nullable: false)
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
                        name: "FK_Items_User_CreatedById1",
                        column: x => x.CreatedById1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 18, 18, 29, 44, 415, DateTimeKind.Utc).AddTicks(517), "Office and technical equipment", "Equipment" },
                    { 2, new DateTime(2025, 8, 18, 18, 29, 44, 415, DateTimeKind.Utc).AddTicks(521), "Office furniture and fixtures", "Furniture" },
                    { 3, new DateTime(2025, 8, 18, 18, 29, 44, 415, DateTimeKind.Utc).AddTicks(522), "Books and publications", "Books" },
                    { 4, new DateTime(2025, 8, 18, 18, 29, 44, 415, DateTimeKind.Utc).AddTicks(523), "Important documents and files", "Documents" },
                    { 5, new DateTime(2025, 8, 18, 18, 29, 44, 415, DateTimeKind.Utc).AddTicks(523), "Miscellaneous items", "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedById1",
                table: "Comments",
                column: "CreatedById1");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_InventoryId",
                table: "Comments",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatedById1",
                table: "Inventories",
                column: "CreatedById1");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_InventoryId",
                table: "InventoryAccesses",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId1",
                table: "InventoryAccesses",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTags_TagId",
                table: "InventoryTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedById1",
                table: "Items",
                column: "CreatedById1");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryId_CustomId",
                table: "Items",
                columns: new[] { "InventoryId", "CustomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_ItemId_UserId",
                table: "Likes",
                columns: new[] { "ItemId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId1",
                table: "Likes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
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
                name: "InventoryCustomIdFormats");

            migrationBuilder.DropTable(
                name: "InventoryFieldConfigurations");

            migrationBuilder.DropTable(
                name: "InventoryTags");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
