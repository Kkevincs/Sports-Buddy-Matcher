using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsBuddyMatcher.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddEventMatchingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Event",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkillId",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkillLevel",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_SkillId",
                table: "Event",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_Skill_SkillId",
                table: "Event",
                column: "SkillId",
                principalTable: "Skill",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_Skill_SkillId",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Event_SkillId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "Event");
        }
    }
}
