using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Activated_To_Active_In_Department_And_Faculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Activated",
                table: "Faculties",
                newName: "Active");

            migrationBuilder.RenameColumn(
                name: "Activated",
                table: "Departments",
                newName: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Faculties",
                newName: "Activated");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Departments",
                newName: "Activated");
        }
    }
}