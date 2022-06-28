using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Changed_StudentId_To_Guid_In_Student_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Students_StudentId1",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_StudentId1",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "Addresses");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StudentId",
                table: "Addresses",
                column: "StudentId",
                unique: true,
                filter: "[StudentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Students_StudentId",
                table: "Addresses",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Students_StudentId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_StudentId",
                table: "Addresses");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StudentId1",
                table: "Addresses",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Students_StudentId1",
                table: "Addresses",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
