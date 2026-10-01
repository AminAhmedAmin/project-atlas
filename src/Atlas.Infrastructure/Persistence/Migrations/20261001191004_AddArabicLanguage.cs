using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArabicLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing content was written in English, so existing rows default to "English".
            migrationBuilder.DropIndex(
                name: "IX_Services_IsPublished_DisplayOrder",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_PageContents_Key",
                table: "PageContents");

            migrationBuilder.DropIndex(
                name: "IX_ContentBlocks_Kind_IsPublished_DisplayOrder",
                table: "ContentBlocks");

            migrationBuilder.AddColumn<string>(
                name: "ArabicCompanyName",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArabicTagline",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Services",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "English");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PageContents",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "English");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ContentBlocks",
                type: "varchar(16)",
                unicode: false,
                maxLength: 16,
                nullable: false,
                defaultValue: "English");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Language_IsPublished_DisplayOrder",
                table: "Services",
                columns: new[] { "Language", "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PageContents_Key_Language",
                table: "PageContents",
                columns: new[] { "Key", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlocks_Language_Kind_IsPublished_DisplayOrder",
                table: "ContentBlocks",
                columns: new[] { "Language", "Kind", "IsPublished", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Language_IsPublished_DisplayOrder",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_PageContents_Key_Language",
                table: "PageContents");

            migrationBuilder.DropIndex(
                name: "IX_ContentBlocks_Language_Kind_IsPublished_DisplayOrder",
                table: "ContentBlocks");

            migrationBuilder.DropColumn(
                name: "ArabicCompanyName",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ArabicTagline",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PageContents");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "ContentBlocks");

            migrationBuilder.CreateIndex(
                name: "IX_Services_IsPublished_DisplayOrder",
                table: "Services",
                columns: new[] { "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PageContents_Key",
                table: "PageContents",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlocks_Kind_IsPublished_DisplayOrder",
                table: "ContentBlocks",
                columns: new[] { "Kind", "IsPublished", "DisplayOrder" });
        }
    }
}
