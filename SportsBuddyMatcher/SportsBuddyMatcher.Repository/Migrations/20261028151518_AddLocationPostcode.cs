using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBuddyMatcher.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationPostcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Location");
        }
    }
}
