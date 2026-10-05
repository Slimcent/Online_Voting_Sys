using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineVoting.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateElectionStatusLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ElectionStatuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [ElectionStatuses])
                BEGIN
                    DECLARE @RegistrationOpenId int =
                    (
                        SELECT TOP(1) [Id]
                        FROM [ElectionStatuses]
                        WHERE [Name] IN (N'Registration Open', N'Application Open')
                        ORDER BY CASE WHEN [Name] = N'Registration Open' THEN 0 ELSE 1 END
                    );

                    DECLARE @RegistrationClosedId int =
                    (
                        SELECT TOP(1) [Id]
                        FROM [ElectionStatuses]
                        WHERE [Name] IN (N'Registration Closed', N'Voter Registration Closed')
                        ORDER BY CASE WHEN [Name] = N'Registration Closed' THEN 0 ELSE 1 END
                    );

                    DECLARE @CompletedId int =
                    (
                        SELECT TOP(1) [Id]
                        FROM [ElectionStatuses]
                        WHERE [Name] = N'Completed'
                    );

                    IF @RegistrationOpenId IS NULL
                        OR @RegistrationClosedId IS NULL
                        OR @CompletedId IS NULL
                    BEGIN
                        THROW 50001, 'Expected election statuses were not found.', 1;
                    END;

                    UPDATE [Elections]
                    SET [ElectionStatusId] = @RegistrationOpenId
                    WHERE [ElectionStatusId] IN
                    (
                        SELECT [Id]
                        FROM [ElectionStatuses]
                        WHERE [Name] IN (N'Application Closed', N'Voter Registration Open')
                    );

                    UPDATE [Elections]
                    SET [ElectionStatusId] = @CompletedId
                    WHERE [ElectionStatusId] IN
                    (
                        SELECT [Id]
                        FROM [ElectionStatuses]
                        WHERE [Name] = N'Voting Closed'
                    );

                    DELETE FROM [ElectionStatuses]
                    WHERE [Name] IN
                    (
                        N'Application Closed',
                        N'Voter Registration Open',
                        N'Voting Closed'
                    );

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'DRAFT',
                        [Name] = N'Draft',
                        [Description] = N'The election has been created but is not yet operational.'
                    WHERE [Name] = N'Draft';

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'REGISTRATION_OPEN',
                        [Name] = N'Registration Open',
                        [Description] = N'Candidate applications and/or voter registration are currently open according to the configured election periods.'
                    WHERE [Id] = @RegistrationOpenId;

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'REGISTRATION_CLOSED',
                        [Name] = N'Registration Closed',
                        [Description] = N'Candidate applications and voter registration have closed.'
                    WHERE [Id] = @RegistrationClosedId;

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'VOTING_OPEN',
                        [Name] = N'Voting Open',
                        [Description] = N'Voting is currently open.'
                    WHERE [Name] = N'Voting Open';

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'COMPLETED',
                        [Name] = N'Completed',
                        [Description] = N'The election has been completed.'
                    WHERE [Id] = @CompletedId;

                    UPDATE [ElectionStatuses]
                    SET
                        [Code] = N'CANCELLED',
                        [Name] = N'Cancelled',
                        [Description] = N'The election has been cancelled.'
                    WHERE [Name] = N'Cancelled';

                    IF EXISTS
                    (
                        SELECT 1
                        FROM [ElectionStatuses]
                        WHERE [Code] IS NULL
                    )
                    BEGIN
                        THROW 50002, 'An unexpected election status exists and could not be assigned a lifecycle code.', 1;
                    END;
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ElectionStatuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectionStatuses_Code",
                table: "ElectionStatuses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ElectionStatuses_Code",
                table: "ElectionStatuses");

            migrationBuilder.Sql("""
                UPDATE [ElectionStatuses]
                SET
                    [Name] = N'Application Open',
                    [Description] = N'Applications for election positions are open.'
                WHERE [Code] = N'REGISTRATION_OPEN';

                UPDATE [ElectionStatuses]
                SET
                    [Name] = N'Voter Registration Closed',
                    [Description] = N'Voter registration is closed.'
                WHERE [Code] = N'REGISTRATION_CLOSED';

                UPDATE [ElectionStatuses]
                SET
                    [Description] = N'The election has been created but is not yet open.'
                WHERE [Code] = N'DRAFT';

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [ElectionStatuses]
                    WHERE [Name] = N'Application Closed'
                )
                BEGIN
                    INSERT INTO [ElectionStatuses]
                    (
                        [Code],
                        [Name],
                        [Description],
                        [Active],
                        [CreatedAt],
                        [UpdatedAt]
                    )
                    VALUES
                    (
                        N'LEGACY_APPLICATION_CLOSED',
                        N'Application Closed',
                        N'Applications for election positions are closed.',
                        1,
                        SYSUTCDATETIME(),
                        SYSUTCDATETIME()
                    );
                END;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [ElectionStatuses]
                    WHERE [Name] = N'Voter Registration Open'
                )
                BEGIN
                    INSERT INTO [ElectionStatuses]
                    (
                        [Code],
                        [Name],
                        [Description],
                        [Active],
                        [CreatedAt],
                        [UpdatedAt]
                    )
                    VALUES
                    (
                        N'LEGACY_VOTER_REGISTRATION_OPEN',
                        N'Voter Registration Open',
                        N'Voter registration is open.',
                        1,
                        SYSUTCDATETIME(),
                        SYSUTCDATETIME()
                    );
                END;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [ElectionStatuses]
                    WHERE [Name] = N'Voting Closed'
                )
                BEGIN
                    INSERT INTO [ElectionStatuses]
                    (
                        [Code],
                        [Name],
                        [Description],
                        [Active],
                        [CreatedAt],
                        [UpdatedAt]
                    )
                    VALUES
                    (
                        N'LEGACY_VOTING_CLOSED',
                        N'Voting Closed',
                        N'Voting has ended.',
                        1,
                        SYSUTCDATETIME(),
                        SYSUTCDATETIME()
                    );
                END;
                """);

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ElectionStatuses");
        }
    }
}
