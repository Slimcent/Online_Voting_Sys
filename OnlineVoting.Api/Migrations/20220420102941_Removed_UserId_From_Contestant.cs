using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Removed_UserId_From_Contestant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contestans_AspNetUsers_UserId",
                table: "Contestans");

            migrationBuilder.DropIndex(
                name: "IX_Contestans_UserId",
                table: "Contestans");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Contestans");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Contestans",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contestans_UserId",
                table: "Contestans",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contestans_AspNetUsers_UserId",
                table: "Contestans",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
