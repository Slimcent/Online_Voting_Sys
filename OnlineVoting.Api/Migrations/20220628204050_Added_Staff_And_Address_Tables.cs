using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_Staff_And_Address_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "AspNetUsers",
                newName: "IsActive");

            migrationBuilder.CreateTable(
                name: "StaffProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffProfile_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlotNo = table.Column<int>(type: "int", nullable: true),
                    StreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_StaffProfile_StaffId",
                        column: x => x.StaffId,
                        principalTable: "StaffProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Addresses_Students_StudentId1",
                        column: x => x.StudentId1,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StaffId",
                table: "Addresses",
                column: "StaffId",
                unique: true,
                filter: "[StaffId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StudentId1",
                table: "Addresses",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfile_UserId",
                table: "StaffProfile",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "StaffProfile");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AspNetUsers",
                newName: "IsDeleted");
        }
    }
}
