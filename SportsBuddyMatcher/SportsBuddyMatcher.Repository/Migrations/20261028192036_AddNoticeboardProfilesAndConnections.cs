using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBuddyMatcher.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddNoticeboardProfilesAndConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "PreferredRange", table: "User", newName: "PreferredRangeKm");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "User",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(name: "PreferredRangeKm", table: "User", type: "decimal(12,2)", precision: 12, scale: 2, nullable: true, oldClrType: typeof(int), oldType: "int", oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "User",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Event",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                table: "Event",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Connection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OtherUserId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connection", x => x.Id);
                    table.CheckConstraint("CK_Connection_Order", "[UserId] < [OtherUserId]");
                    table.ForeignKey(
                        name: "FK_Connection_User_OtherUserId",
                        column: x => x.OtherUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Connection_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_CreatedById",
                table: "Event",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Connection_OtherUserId",
                table: "Connection",
                column: "OtherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Connection_UserId_OtherUserId",
                table: "Connection",
                columns: new[] { "UserId", "OtherUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Event_User_CreatedById",
                table: "Event",
                column: "CreatedById",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_User_CreatedById",
                table: "Event");

            migrationBuilder.DropTable(
                name: "Connection");

            migrationBuilder.DropIndex(
                name: "IX_Event_CreatedById",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "User");



            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "PictureUrl",
                table: "Event");

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [User] WHERE PreferredRangeKm <> FLOOR(PreferredRangeKm)) THROW 50000, 'Fractional ranges cannot be converted to integers without data loss.', 1;"); migrationBuilder.AlterColumn<int>(name: "PreferredRangeKm", table: "User", type: "int", nullable: true, oldClrType: typeof(decimal), oldType: "decimal(12,2)", oldNullable: true); migrationBuilder.RenameColumn(name: "PreferredRangeKm", table: "User", newName: "PreferredRange");
        }
    }
}
