using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddElectionScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectionScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionScopes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectionScopes_Code",
                table: "ElectionScopes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionScopes_Name",
                table: "ElectionScopes",
                column: "Name",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO [ElectionScopes]
                    ([Code], [Name], [Description], [Active], [CreatedAt], [UpdatedAt])
                VALUES
                    (N'UNIVERSITY', N'University', N'Applies across the university.', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
                    (N'FACULTY', N'Faculty', N'Applies within a faculty.', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
                    (N'DEPARTMENT', N'Department', N'Applies within a department.', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                """);

            migrationBuilder.AddColumn<int>(
                name: "ElectionScopeId",
                table: "ElectionTypes",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE electionType
                SET electionType.ElectionScopeId = electionScope.Id
                FROM [ElectionTypes] electionType
                INNER JOIN [ElectionScopes] electionScope
                    ON electionScope.Code =
                        CASE electionType.Name
                            WHEN N'University Election' THEN N'UNIVERSITY'
                            WHEN N'Faculty Election' THEN N'FACULTY'
                            WHEN N'Department Election' THEN N'DEPARTMENT'
                        END
                WHERE electionType.Name IN
                (
                    N'University Election',
                    N'Faculty Election',
                    N'Department Election'
                );
                """);

            migrationBuilder.Sql("""
                IF EXISTS
                (
                    SELECT 1
                    FROM [ElectionTypes]
                    WHERE [ElectionScopeId] IS NULL
                )
                BEGIN
                    THROW 50000, 'An election type exists without an election scope assignment.', 1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ElectionScopeId",
                table: "ElectionTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionTypes_ElectionScopeId",
                table: "ElectionTypes",
                column: "ElectionScopeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectionTypes_ElectionScopes_ElectionScopeId",
                table: "ElectionTypes",
                column: "ElectionScopeId",
                principalTable: "ElectionScopes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ElectionTypes_ElectionScopes_ElectionScopeId",
                table: "ElectionTypes");

            migrationBuilder.DropTable(
                name: "ElectionScopes");

            migrationBuilder.DropIndex(
                name: "IX_ElectionTypes_ElectionScopeId",
                table: "ElectionTypes");

            migrationBuilder.DropColumn(
                name: "ElectionScopeId",
                table: "ElectionTypes");
        }
    }
}
