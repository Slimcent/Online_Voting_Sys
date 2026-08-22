using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class Added_Active_To_Contestant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contestans_Positions_PositionId",
                table: "Contestans");

            migrationBuilder.DropForeignKey(
                name: "FK_Contestans_Students_StudentId",
                table: "Contestans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contestans",
                table: "Contestans");

            migrationBuilder.RenameTable(
                name: "Contestans",
                newName: "Contestants");

            migrationBuilder.RenameIndex(
                name: "IX_Contestans_StudentId",
                table: "Contestants",
                newName: "IX_Contestants_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Contestans_PositionId",
                table: "Contestants",
                newName: "IX_Contestants_PositionId");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Contestants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contestants_Positions_PositionId",
                table: "Contestants",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contestants_Students_StudentId",
                table: "Contestants",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_Positions_PositionId",
                table: "Contestants");

            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_Students_StudentId",
                table: "Contestants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Contestants");

            migrationBuilder.RenameTable(
                name: "Contestants",
                newName: "Contestans");

            migrationBuilder.RenameIndex(
                name: "IX_Contestants_StudentId",
                table: "Contestans",
                newName: "IX_Contestans_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Contestants_PositionId",
                table: "Contestans",
                newName: "IX_Contestans_PositionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contestans",
                table: "Contestans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contestans_Positions_PositionId",
                table: "Contestans",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contestans_Students_StudentId",
                table: "Contestans",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
