using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TgTranslator.Migrations
{
    /// <inheritdoc />
    public partial class Delay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "delay",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delay",
                table: "groups");
        }
    }
}
