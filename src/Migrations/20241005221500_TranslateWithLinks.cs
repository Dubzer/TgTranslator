using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TgTranslator.Migrations
{
    /// <inheritdoc />
    public partial class TranslateWithLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "translate_with_links",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "translate_with_links",
                table: "groups");
        }
    }
}
