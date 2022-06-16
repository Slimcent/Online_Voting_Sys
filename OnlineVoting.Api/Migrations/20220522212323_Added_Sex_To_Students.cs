using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    public partial class Added_Sex_To_Students : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Students");
        }
    }
}
