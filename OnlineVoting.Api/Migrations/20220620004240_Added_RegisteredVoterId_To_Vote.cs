using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_RegisteredVoterId_To_Vote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Votes");

            migrationBuilder.AddColumn<Guid>(
                name: "RegisteredVoterId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Votes_RegisteredVoterId",
                table: "Votes",
                column: "RegisteredVoterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_RegisteredVoter_RegisteredVoterId",
                table: "Votes",
                column: "RegisteredVoterId",
                principalTable: "RegisteredVoter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_RegisteredVoter_RegisteredVoterId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_RegisteredVoterId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "RegisteredVoterId",
                table: "Votes");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
