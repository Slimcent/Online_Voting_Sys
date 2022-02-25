using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class EditedContestantTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "RegNo",
                table: "Contestants");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Contestants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Contestants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Contestants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contestants_StudentId",
                table: "Contestants",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Contestants_UserId1",
                table: "Contestants",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Contestants_AspNetUsers_UserId1",
                table: "Contestants",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contestants_Students_StudentId",
                table: "Contestants",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_AspNetUsers_UserId1",
                table: "Contestants");

            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_Students_StudentId",
                table: "Contestants");

            migrationBuilder.DropIndex(
                name: "IX_Contestants_StudentId",
                table: "Contestants");

            migrationBuilder.DropIndex(
                name: "IX_Contestants_UserId1",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Contestants");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Contestants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Contestants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Contestants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegNo",
                table: "Contestants",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
