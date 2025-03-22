using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TgTranslator.Migrations
{
    /// <inheritdoc />
    public partial class MultipleLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table groups
                                     alter column language type text[]
                                         using array[language]::text[];
                                 
                                 alter table groups
                                     rename column language TO languages;
                                 """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 alter table groups
                                     alter column languages TYPE text
                                         using languages[1];
                                 
                                 alter table groups
                                     rename column languages TO language;
                                 """);
        }
    }
}
