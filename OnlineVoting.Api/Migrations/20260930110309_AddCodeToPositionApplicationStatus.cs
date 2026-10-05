using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToPositionApplicationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PositionApplicationStatuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE PositionApplicationStatuses
                SET Code = 'PENDING_PAYMENT'
                WHERE Name = 'Pending Payment';

                UPDATE PositionApplicationStatuses
                SET Code = 'PENDING_REVIEW'
                WHERE Name = 'Pending Review';

                UPDATE PositionApplicationStatuses
                SET Code = 'APPROVED'
                WHERE Name = 'Approved';

                UPDATE PositionApplicationStatuses
                SET Code = 'REJECTED'
                WHERE Name = 'Rejected';

                UPDATE PositionApplicationStatuses
                SET Code = 'WITHDRAWN'
                WHERE Name = 'Withdrawn';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "PositionApplicationStatuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionApplicationStatuses_Code",
                table: "PositionApplicationStatuses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PositionApplicationStatuses_Code",
                table: "PositionApplicationStatuses");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PositionApplicationStatuses");
        }
    }
}
