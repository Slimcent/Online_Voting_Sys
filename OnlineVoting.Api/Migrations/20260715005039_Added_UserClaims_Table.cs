using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_UserClaims_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "AspNetUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspNetUserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspNetUserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "AspNetUserClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspNetUserClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspNetUserClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "AspNetRoleClaims",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoleClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AspNetRoleClaims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetRoleClaims",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AspNetRoleClaims",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetRoleClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AspNetRoleClaims");
        }
    }
}
