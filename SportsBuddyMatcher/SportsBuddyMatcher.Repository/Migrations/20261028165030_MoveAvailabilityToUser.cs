using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBuddyMatcher.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MoveAvailabilityToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Availability_PlayerSkill_PlayerSkillId",
                table: "Availability");

            migrationBuilder.RenameColumn(
                name: "PlayerSkillId",
                table: "Availability",
                newName: "UserId");

            migrationBuilder.Sql(@"
                UPDATE a
                SET a.UserId = p.UserId
                FROM Availability a
                INNER JOIN PlayerSkill p ON a.UserId = p.Id;");

            migrationBuilder.RenameIndex(
                name: "IX_Availability_PlayerSkillId",
                table: "Availability",
                newName: "IX_Availability_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Availability_User_UserId",
                table: "Availability",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Restoring sport-specific availability requires an explicit data migration.");
        }
    }
}
