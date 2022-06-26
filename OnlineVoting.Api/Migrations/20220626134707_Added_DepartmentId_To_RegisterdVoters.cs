using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_DepartmentId_To_RegisterdVoters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RegisteredVoter",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RegisteredVoter",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "RegisteredVoter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RegisteredVoter",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "RegisteredVoter",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoter_DepartmentId",
                table: "RegisteredVoter",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredVoter_Departments_DepartmentId",
                table: "RegisteredVoter",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredVoter_Departments_DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoter_DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RegisteredVoter");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");
        }
    }
}
