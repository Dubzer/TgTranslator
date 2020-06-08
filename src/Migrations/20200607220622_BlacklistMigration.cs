using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TgTranslator.Migrations
{
    public partial class BlacklistMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "groups_pkey",
                table: "groups");

            migrationBuilder.AddPrimaryKey(
                name: "groups_pk",
                table: "groups",
                column: "group_id");

            migrationBuilder.CreateTable(
                name: "groups_blacklist",
                columns: table => new
                {
                    group_id = table.Column<long>(nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc'::text, now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("groups_blacklist_pk", x => x.group_id);
                    table.ForeignKey(
                        name: "groups_blacklist_groups_group_id_fk",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "group_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "groups_blacklist_group_id_uindex",
                table: "groups_blacklist",
                column: "group_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "groups_blacklist");

            migrationBuilder.DropPrimaryKey(
                name: "groups_pk",
                table: "groups");

            migrationBuilder.AddPrimaryKey(
                name: "groups_pkey",
                table: "groups",
                column: "group_id");
        }
    }
}
