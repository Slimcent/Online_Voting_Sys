using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class RedesignElectionDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM [Votes])
                    OR EXISTS (SELECT 1 FROM [Contestants])
                    OR EXISTS (SELECT 1 FROM [RegisteredVoter])
                    OR EXISTS (SELECT 1 FROM [Positions])
                BEGIN
                    THROW 50003, 'Election domain migration requires the legacy voting tables to be empty.', 1;
                END
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_Positions_PositionId",
                table: "Contestants");

            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_Students_StudentId",
                table: "Contestants");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredVoter_Departments_DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredVoter_Students_StudentId",
                table: "RegisteredVoter");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_RegisteredVoter_RegisteredVoterId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Students_StudentId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_RegisteredVoterId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_StudentId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Contestants_PositionId",
                table: "Contestants");

            migrationBuilder.DropIndex(
                name: "IX_Contestants_StudentId",
                table: "Contestants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredVoter",
                table: "RegisteredVoter");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoter_DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoter_StudentId",
                table: "RegisteredVoter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "HasVoted",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "VoterId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "RegisteredVoter");

            migrationBuilder.RenameTable(
                name: "RegisteredVoter",
                newName: "RegisteredVoters");

            migrationBuilder.RenameColumn(
                name: "IsDeActivated",
                table: "RegisteredVoters",
                newName: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "RegisteredVoterId",
                table: "Votes",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "ContestantId",
                table: "Votes",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Votes",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Votes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ElectionPositionId",
                table: "Votes",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Votes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Votes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Positions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Positions",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<long>(
                name: "DepartmentId",
                table: "Positions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FacultyId",
                table: "Positions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Contestants",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PositionApplicationId",
                table: "Contestants",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants",
                column: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "VotingCode",
                table: "RegisteredVoters",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "RegisteredVoters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "RegisteredVoters",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ElectionId",
                table: "RegisteredVoters",
                type: "varchar(36)",
                unicode: false,
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredVoters",
                table: "RegisteredVoters",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ElectionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_ElectionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_ElectionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PositionApplicationStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_PositionApplicationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    ElectionTypeId = table.Column<int>(type: "int", nullable: false),
                    ElectionStatusId = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<long>(type: "bigint", nullable: true),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: true),
                    ApplicationStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicationEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoterRegistrationStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VoterRegistrationEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VotingStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VotingEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elections", x => x.Id);
                    table.CheckConstraint("CK_Elections_ApplicationPeriod", "([ApplicationStartAt] IS NULL AND [ApplicationEndAt] IS NULL) OR ([ApplicationStartAt] IS NOT NULL AND [ApplicationEndAt] IS NOT NULL AND [ApplicationEndAt] > [ApplicationStartAt])");
                    table.CheckConstraint("CK_Elections_Scope", "[FacultyId] IS NULL OR [DepartmentId] IS NULL");
                    table.CheckConstraint("CK_Elections_VoterRegistrationPeriod", "([VoterRegistrationStartAt] IS NULL AND [VoterRegistrationEndAt] IS NULL) OR ([VoterRegistrationStartAt] IS NOT NULL AND [VoterRegistrationEndAt] IS NOT NULL AND [VoterRegistrationEndAt] > [VoterRegistrationStartAt])");
                    table.CheckConstraint("CK_Elections_VotingPeriod", "([VotingStartAt] IS NULL AND [VotingEndAt] IS NULL) OR ([VotingStartAt] IS NOT NULL AND [VotingEndAt] IS NOT NULL AND [VotingEndAt] > [VotingStartAt])");
                    table.ForeignKey(
                        name: "FK_Elections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Elections_ElectionStatuses_ElectionStatusId",
                        column: x => x.ElectionStatusId,
                        principalTable: "ElectionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Elections_ElectionTypes_ElectionTypeId",
                        column: x => x.ElectionTypeId,
                        principalTable: "ElectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Elections_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Elections_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ElectionPositions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    ElectionId = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    PositionId = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    ApplicationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectionPositions", x => x.Id);
                    table.CheckConstraint("CK_ElectionPositions_ApplicationFee", "[ApplicationFee] >= 0");
                    table.ForeignKey(
                        name: "FK_ElectionPositions_Elections_ElectionId",
                        column: x => x.ElectionId,
                        principalTable: "Elections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectionPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PositionApplications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ElectionPositionId = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false),
                    PositionApplicationStatusId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionApplications_ElectionPositions_ElectionPositionId",
                        column: x => x.ElectionPositionId,
                        principalTable: "ElectionPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionApplications_PositionApplicationStatuses_PositionApplicationStatusId",
                        column: x => x.PositionApplicationStatusId,
                        principalTable: "PositionApplicationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionApplications_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ContestantId",
                table: "Votes",
                column: "ContestantId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ElectionPositionId",
                table: "Votes",
                column: "ElectionPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_RegisteredVoterId_ElectionPositionId",
                table: "Votes",
                columns: new[] { "RegisteredVoterId", "ElectionPositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentId_Name",
                table: "Positions",
                columns: new[] { "DepartmentId", "Name" },
                unique: true,
                filter: "[DepartmentId] IS NOT NULL AND [FacultyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_FacultyId_Name",
                table: "Positions",
                columns: new[] { "FacultyId", "Name" },
                unique: true,
                filter: "[FacultyId] IS NOT NULL AND [DepartmentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Name",
                table: "Positions",
                column: "Name",
                unique: true,
                filter: "[FacultyId] IS NULL AND [DepartmentId] IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Positions_Scope",
                table: "Positions",
                sql: "[FacultyId] IS NULL OR [DepartmentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contestants_PositionApplicationId",
                table: "Contestants",
                column: "PositionApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoters_ElectionId",
                table: "RegisteredVoters",
                column: "ElectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoters_StudentId_ElectionId",
                table: "RegisteredVoters",
                columns: new[] { "StudentId", "ElectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoters_VotingCode",
                table: "RegisteredVoters",
                column: "VotingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionPositions_ElectionId_PositionId",
                table: "ElectionPositions",
                columns: new[] { "ElectionId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionPositions_PositionId",
                table: "ElectionPositions",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_DepartmentId_ElectionTypeId_YearId",
                table: "Elections",
                columns: new[] { "DepartmentId", "ElectionTypeId", "YearId" },
                unique: true,
                filter: "[DepartmentId] IS NOT NULL AND [FacultyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_ElectionStatusId",
                table: "Elections",
                column: "ElectionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_ElectionTypeId_YearId",
                table: "Elections",
                columns: new[] { "ElectionTypeId", "YearId" },
                unique: true,
                filter: "[FacultyId] IS NULL AND [DepartmentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_FacultyId_ElectionTypeId_YearId",
                table: "Elections",
                columns: new[] { "FacultyId", "ElectionTypeId", "YearId" },
                unique: true,
                filter: "[FacultyId] IS NOT NULL AND [DepartmentId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Elections_YearId",
                table: "Elections",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectionStatuses_Name",
                table: "ElectionStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionTypes_Name",
                table: "ElectionTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionApplications_ElectionPositionId",
                table: "PositionApplications",
                column: "ElectionPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionApplications_PositionApplicationStatusId",
                table: "PositionApplications",
                column: "PositionApplicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionApplications_StudentId_ElectionPositionId",
                table: "PositionApplications",
                columns: new[] { "StudentId", "ElectionPositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionApplicationStatuses_Name",
                table: "PositionApplicationStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Years_Name",
                table: "Years",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contestants_PositionApplications_PositionApplicationId",
                table: "Contestants",
                column: "PositionApplicationId",
                principalTable: "PositionApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Faculties_FacultyId",
                table: "Positions",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredVoters_Elections_ElectionId",
                table: "RegisteredVoters",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredVoters_Students_StudentId",
                table: "RegisteredVoters",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Contestants_ContestantId",
                table: "Votes",
                column: "ContestantId",
                principalTable: "Contestants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_ElectionPositions_ElectionPositionId",
                table: "Votes",
                column: "ElectionPositionId",
                principalTable: "ElectionPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_RegisteredVoters_RegisteredVoterId",
                table: "Votes",
                column: "RegisteredVoterId",
                principalTable: "RegisteredVoters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contestants_PositionApplications_PositionApplicationId",
                table: "Contestants");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Faculties_FacultyId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredVoters_Elections_ElectionId",
                table: "RegisteredVoters");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisteredVoters_Students_StudentId",
                table: "RegisteredVoters");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Contestants_ContestantId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_ElectionPositions_ElectionPositionId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_RegisteredVoters_RegisteredVoterId",
                table: "Votes");

            migrationBuilder.DropTable(
                name: "PositionApplications");

            migrationBuilder.DropTable(
                name: "ElectionPositions");

            migrationBuilder.DropTable(
                name: "PositionApplicationStatuses");

            migrationBuilder.DropTable(
                name: "Elections");

            migrationBuilder.DropTable(
                name: "ElectionStatuses");

            migrationBuilder.DropTable(
                name: "ElectionTypes");

            migrationBuilder.DropTable(
                name: "Years");

            migrationBuilder.DropIndex(
                name: "IX_Votes_ContestantId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_ElectionPositionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_RegisteredVoterId_ElectionPositionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Positions_DepartmentId_Name",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_FacultyId_Name",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_Name",
                table: "Positions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Positions_Scope",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Contestants_PositionApplicationId",
                table: "Contestants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredVoters",
                table: "RegisteredVoters");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoters_ElectionId",
                table: "RegisteredVoters");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoters_StudentId_ElectionId",
                table: "RegisteredVoters");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredVoters_VotingCode",
                table: "RegisteredVoters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "ElectionPositionId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionApplicationId",
                table: "Contestants");

            migrationBuilder.DropColumn(
                name: "ElectionId",
                table: "RegisteredVoters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Positions",
                table: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants");

            migrationBuilder.RenameTable(
                name: "RegisteredVoters",
                newName: "RegisteredVoter");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "RegisteredVoter",
                newName: "IsDeActivated");

            migrationBuilder.AlterColumn<Guid>(
                name: "RegisteredVoterId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "ContestantId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<bool>(
                name: "HasVoted",
                table: "Votes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoterId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Contestants",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "Contestants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Contestants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Positions",
                table: "Positions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contestants",
                table: "Contestants",
                column: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "VotingCode",
                table: "RegisteredVoter",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "RegisteredVoter",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "RegisteredVoter",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldUnicode: false,
                oldMaxLength: 36);

            migrationBuilder.AddColumn<long>(
                name: "DepartmentId",
                table: "RegisteredVoter",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredVoter",
                table: "RegisteredVoter",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_RegisteredVoterId",
                table: "Votes",
                column: "RegisteredVoterId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_StudentId",
                table: "Votes",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Contestants_PositionId",
                table: "Contestants",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Contestants_StudentId",
                table: "Contestants",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoter_DepartmentId",
                table: "RegisteredVoter",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredVoter_StudentId",
                table: "RegisteredVoter",
                column: "StudentId",
                unique: true,
                filter: "[StudentId] IS NOT NULL");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredVoter_Departments_DepartmentId",
                table: "RegisteredVoter",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegisteredVoter_Students_StudentId",
                table: "RegisteredVoter",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_RegisteredVoter_RegisteredVoterId",
                table: "Votes",
                column: "RegisteredVoterId",
                principalTable: "RegisteredVoter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Students_StudentId",
                table: "Votes",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }
    }
}
