using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_UserType_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Menus",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Menus", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            //migrationBuilder.CreateTable(
            //    name: "Claims",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Claim = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MenuId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MenuId1 = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Claims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Claims_Menus_MenuId1",
            //            column: x => x.MenuId1,
            //            principalTable: "Menus",
            //            principalColumn: "Id");
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserTypeId",
                table: "AspNetUsers",
                column: "UserTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Claims_MenuId1",
            //    table: "Claims",
            //    column: "MenuId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_UserTypes_UserTypeId",
                table: "AspNetUsers",
                column: "UserTypeId",
                principalTable: "UserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_UserTypes_UserTypeId",
                table: "AspNetUsers");

            //migrationBuilder.DropTable(
            //    name: "Claims");

            migrationBuilder.DropTable(
                name: "UserTypes");

            //migrationBuilder.DropTable(
            //    name: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserTypeId",
                table: "AspNetUsers");
        }
    }
}
