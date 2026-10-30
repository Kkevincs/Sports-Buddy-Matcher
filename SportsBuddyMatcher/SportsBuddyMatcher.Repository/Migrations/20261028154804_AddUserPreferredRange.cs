using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBuddyMatcher.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferredRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredRange",
                table: "User",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredRange",
                table: "User");
        }
    }
}
