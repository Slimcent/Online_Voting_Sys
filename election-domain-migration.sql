BEGIN TRANSACTION;

                IF EXISTS (SELECT 1 FROM [Votes])
                    OR EXISTS (SELECT 1 FROM [Contestants])
                    OR EXISTS (SELECT 1 FROM [RegisteredVoter])
                    OR EXISTS (SELECT 1 FROM [Positions])
                BEGIN
                    THROW 50003, 'Election domain migration requires the legacy voting tables to be empty.', 1;
                END
            

ALTER TABLE [Contestants] DROP CONSTRAINT [FK_Contestants_Positions_PositionId];

ALTER TABLE [Contestants] DROP CONSTRAINT [FK_Contestants_Students_StudentId];

ALTER TABLE [RegisteredVoter] DROP CONSTRAINT [FK_RegisteredVoter_Departments_DepartmentId];

ALTER TABLE [RegisteredVoter] DROP CONSTRAINT [FK_RegisteredVoter_Students_StudentId];

ALTER TABLE [Votes] DROP CONSTRAINT [FK_Votes_RegisteredVoter_RegisteredVoterId];

ALTER TABLE [Votes] DROP CONSTRAINT [FK_Votes_Students_StudentId];

DROP INDEX [IX_Votes_RegisteredVoterId] ON [Votes];

DROP INDEX [IX_Votes_StudentId] ON [Votes];

DROP INDEX [IX_Contestants_PositionId] ON [Contestants];

DROP INDEX [IX_Contestants_StudentId] ON [Contestants];

ALTER TABLE [RegisteredVoter] DROP CONSTRAINT [PK_RegisteredVoter];

DROP INDEX [IX_RegisteredVoter_DepartmentId] ON [RegisteredVoter];

DROP INDEX [IX_RegisteredVoter_StudentId] ON [RegisteredVoter];

ALTER TABLE [Votes] DROP CONSTRAINT [PK_Votes];

ALTER TABLE [Positions] DROP CONSTRAINT [PK_Positions];

ALTER TABLE [Contestants] DROP CONSTRAINT [PK_Contestants];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'HasVoted');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Votes] DROP COLUMN [HasVoted];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'StudentId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Votes] DROP COLUMN [StudentId];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'VoterId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Votes] DROP COLUMN [VoterId];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contestants]') AND [c].[name] = N'PositionId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Contestants] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Contestants] DROP COLUMN [PositionId];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contestants]') AND [c].[name] = N'StudentId');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Contestants] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Contestants] DROP COLUMN [StudentId];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RegisteredVoter]') AND [c].[name] = N'DepartmentId');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [RegisteredVoter] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [RegisteredVoter] DROP COLUMN [DepartmentId];

EXEC sp_rename N'[RegisteredVoter]', N'RegisteredVoters', 'OBJECT';

EXEC sp_rename N'[RegisteredVoters].[IsDeActivated]', N'Active', 'COLUMN';

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'RegisteredVoterId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Votes] ALTER COLUMN [RegisteredVoterId] varchar(36) NOT NULL;

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'ContestantId');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [Votes] ALTER COLUMN [ContestantId] varchar(36) NOT NULL;

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Votes]') AND [c].[name] = N'Id');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Votes] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [Votes] ALTER COLUMN [Id] varchar(36) NOT NULL;

ALTER TABLE [Votes] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Votes] ADD [CreatedBy] nvarchar(max) NULL;

ALTER TABLE [Votes] ADD [ElectionPositionId] varchar(36) NOT NULL DEFAULT '';

ALTER TABLE [Votes] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Votes] ADD [UpdatedBy] nvarchar(max) NULL;

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Positions]') AND [c].[name] = N'Name');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Positions] DROP CONSTRAINT ' + @var9 + ';');
UPDATE [Positions] SET [Name] = N'' WHERE [Name] IS NULL;
ALTER TABLE [Positions] ALTER COLUMN [Name] nvarchar(150) NOT NULL;
ALTER TABLE [Positions] ADD DEFAULT N'' FOR [Name];

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Positions]') AND [c].[name] = N'Id');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Positions] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [Positions] ALTER COLUMN [Id] varchar(36) NOT NULL;

ALTER TABLE [Positions] ADD [DepartmentId] bigint NULL;

ALTER TABLE [Positions] ADD [FacultyId] bigint NULL;

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Contestants]') AND [c].[name] = N'Id');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Contestants] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [Contestants] ALTER COLUMN [Id] varchar(36) NOT NULL;

ALTER TABLE [Contestants] ADD [PositionApplicationId] varchar(36) NOT NULL DEFAULT '';

ALTER TABLE [Votes] ADD CONSTRAINT [PK_Votes] PRIMARY KEY ([Id]);

ALTER TABLE [Positions] ADD CONSTRAINT [PK_Positions] PRIMARY KEY ([Id]);

ALTER TABLE [Contestants] ADD CONSTRAINT [PK_Contestants] PRIMARY KEY ([Id]);

DECLARE @var12 nvarchar(max);
SELECT @var12 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RegisteredVoters]') AND [c].[name] = N'VotingCode');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [RegisteredVoters] DROP CONSTRAINT ' + @var12 + ';');
UPDATE [RegisteredVoters] SET [VotingCode] = '' WHERE [VotingCode] IS NULL;
ALTER TABLE [RegisteredVoters] ALTER COLUMN [VotingCode] varchar(100) NOT NULL;
ALTER TABLE [RegisteredVoters] ADD DEFAULT '' FOR [VotingCode];

DECLARE @var13 nvarchar(max);
SELECT @var13 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RegisteredVoters]') AND [c].[name] = N'StudentId');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [RegisteredVoters] DROP CONSTRAINT ' + @var13 + ';');
UPDATE [RegisteredVoters] SET [StudentId] = '00000000-0000-0000-0000-000000000000' WHERE [StudentId] IS NULL;
ALTER TABLE [RegisteredVoters] ALTER COLUMN [StudentId] uniqueidentifier NOT NULL;
ALTER TABLE [RegisteredVoters] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [StudentId];

DECLARE @var14 nvarchar(max);
SELECT @var14 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RegisteredVoters]') AND [c].[name] = N'Id');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [RegisteredVoters] DROP CONSTRAINT ' + @var14 + ';');
ALTER TABLE [RegisteredVoters] ALTER COLUMN [Id] varchar(36) NOT NULL;

ALTER TABLE [RegisteredVoters] ADD [ElectionId] varchar(36) NOT NULL DEFAULT '';

ALTER TABLE [RegisteredVoters] ADD CONSTRAINT [PK_RegisteredVoters] PRIMARY KEY ([Id]);

CREATE TABLE [ElectionStatuses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ElectionStatuses] PRIMARY KEY ([Id])
);

CREATE TABLE [ElectionTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ElectionTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [PositionApplicationStatuses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(250) NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PositionApplicationStatuses] PRIMARY KEY ([Id])
);

CREATE TABLE [Years] (
    [Id] bigint NOT NULL IDENTITY,
    [Name] nvarchar(20) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Years] PRIMARY KEY ([Id])
);

CREATE TABLE [Elections] (
    [Id] varchar(36) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [YearId] bigint NOT NULL,
    [ElectionTypeId] int NOT NULL,
    [ElectionStatusId] int NOT NULL,
    [FacultyId] bigint NULL,
    [DepartmentId] bigint NULL,
    [ApplicationStartAt] datetime2 NULL,
    [ApplicationEndAt] datetime2 NULL,
    [VoterRegistrationStartAt] datetime2 NULL,
    [VoterRegistrationEndAt] datetime2 NULL,
    [VotingStartAt] datetime2 NULL,
    [VotingEndAt] datetime2 NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Elections] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Elections_ApplicationPeriod] CHECK (([ApplicationStartAt] IS NULL AND [ApplicationEndAt] IS NULL) OR ([ApplicationStartAt] IS NOT NULL AND [ApplicationEndAt] IS NOT NULL AND [ApplicationEndAt] > [ApplicationStartAt])),
    CONSTRAINT [CK_Elections_Scope] CHECK ([FacultyId] IS NULL OR [DepartmentId] IS NULL),
    CONSTRAINT [CK_Elections_VoterRegistrationPeriod] CHECK (([VoterRegistrationStartAt] IS NULL AND [VoterRegistrationEndAt] IS NULL) OR ([VoterRegistrationStartAt] IS NOT NULL AND [VoterRegistrationEndAt] IS NOT NULL AND [VoterRegistrationEndAt] > [VoterRegistrationStartAt])),
    CONSTRAINT [CK_Elections_VotingPeriod] CHECK (([VotingStartAt] IS NULL AND [VotingEndAt] IS NULL) OR ([VotingStartAt] IS NOT NULL AND [VotingEndAt] IS NOT NULL AND [VotingEndAt] > [VotingStartAt])),
    CONSTRAINT [FK_Elections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Elections_ElectionStatuses_ElectionStatusId] FOREIGN KEY ([ElectionStatusId]) REFERENCES [ElectionStatuses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Elections_ElectionTypes_ElectionTypeId] FOREIGN KEY ([ElectionTypeId]) REFERENCES [ElectionTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Elections_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Elections_Years_YearId] FOREIGN KEY ([YearId]) REFERENCES [Years] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ElectionPositions] (
    [Id] varchar(36) NOT NULL,
    [ElectionId] varchar(36) NOT NULL,
    [PositionId] varchar(36) NOT NULL,
    [ApplicationFee] decimal(18,2) NOT NULL,
    [Currency] varchar(3) NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ElectionPositions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ElectionPositions_ApplicationFee] CHECK ([ApplicationFee] >= 0),
    CONSTRAINT [FK_ElectionPositions_Elections_ElectionId] FOREIGN KEY ([ElectionId]) REFERENCES [Elections] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ElectionPositions_Positions_PositionId] FOREIGN KEY ([PositionId]) REFERENCES [Positions] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PositionApplications] (
    [Id] varchar(36) NOT NULL,
    [StudentId] uniqueidentifier NOT NULL,
    [ElectionPositionId] varchar(36) NOT NULL,
    [PositionApplicationStatusId] int NOT NULL,
    [Active] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PositionApplications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PositionApplications_ElectionPositions_ElectionPositionId] FOREIGN KEY ([ElectionPositionId]) REFERENCES [ElectionPositions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PositionApplications_PositionApplicationStatuses_PositionApplicationStatusId] FOREIGN KEY ([PositionApplicationStatusId]) REFERENCES [PositionApplicationStatuses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PositionApplications_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Votes_ContestantId] ON [Votes] ([ContestantId]);

CREATE INDEX [IX_Votes_ElectionPositionId] ON [Votes] ([ElectionPositionId]);

CREATE UNIQUE INDEX [IX_Votes_RegisteredVoterId_ElectionPositionId] ON [Votes] ([RegisteredVoterId], [ElectionPositionId]);

CREATE UNIQUE INDEX [IX_Positions_DepartmentId_Name] ON [Positions] ([DepartmentId], [Name]) WHERE [DepartmentId] IS NOT NULL AND [FacultyId] IS NULL;

CREATE UNIQUE INDEX [IX_Positions_FacultyId_Name] ON [Positions] ([FacultyId], [Name]) WHERE [FacultyId] IS NOT NULL AND [DepartmentId] IS NULL;

CREATE UNIQUE INDEX [IX_Positions_Name] ON [Positions] ([Name]) WHERE [FacultyId] IS NULL AND [DepartmentId] IS NULL;

ALTER TABLE [Positions] ADD CONSTRAINT [CK_Positions_Scope] CHECK ([FacultyId] IS NULL OR [DepartmentId] IS NULL);

CREATE UNIQUE INDEX [IX_Contestants_PositionApplicationId] ON [Contestants] ([PositionApplicationId]);

CREATE INDEX [IX_RegisteredVoters_ElectionId] ON [RegisteredVoters] ([ElectionId]);

CREATE UNIQUE INDEX [IX_RegisteredVoters_StudentId_ElectionId] ON [RegisteredVoters] ([StudentId], [ElectionId]);

CREATE UNIQUE INDEX [IX_RegisteredVoters_VotingCode] ON [RegisteredVoters] ([VotingCode]);

CREATE UNIQUE INDEX [IX_ElectionPositions_ElectionId_PositionId] ON [ElectionPositions] ([ElectionId], [PositionId]);

CREATE INDEX [IX_ElectionPositions_PositionId] ON [ElectionPositions] ([PositionId]);

CREATE UNIQUE INDEX [IX_Elections_DepartmentId_ElectionTypeId_YearId] ON [Elections] ([DepartmentId], [ElectionTypeId], [YearId]) WHERE [DepartmentId] IS NOT NULL AND [FacultyId] IS NULL;

CREATE INDEX [IX_Elections_ElectionStatusId] ON [Elections] ([ElectionStatusId]);

CREATE UNIQUE INDEX [IX_Elections_ElectionTypeId_YearId] ON [Elections] ([ElectionTypeId], [YearId]) WHERE [FacultyId] IS NULL AND [DepartmentId] IS NULL;

CREATE UNIQUE INDEX [IX_Elections_FacultyId_ElectionTypeId_YearId] ON [Elections] ([FacultyId], [ElectionTypeId], [YearId]) WHERE [FacultyId] IS NOT NULL AND [DepartmentId] IS NULL;

CREATE INDEX [IX_Elections_YearId] ON [Elections] ([YearId]);

CREATE UNIQUE INDEX [IX_ElectionStatuses_Name] ON [ElectionStatuses] ([Name]);

CREATE UNIQUE INDEX [IX_ElectionTypes_Name] ON [ElectionTypes] ([Name]);

CREATE INDEX [IX_PositionApplications_ElectionPositionId] ON [PositionApplications] ([ElectionPositionId]);

CREATE INDEX [IX_PositionApplications_PositionApplicationStatusId] ON [PositionApplications] ([PositionApplicationStatusId]);

CREATE UNIQUE INDEX [IX_PositionApplications_StudentId_ElectionPositionId] ON [PositionApplications] ([StudentId], [ElectionPositionId]);

CREATE UNIQUE INDEX [IX_PositionApplicationStatuses_Name] ON [PositionApplicationStatuses] ([Name]);

CREATE UNIQUE INDEX [IX_Years_Name] ON [Years] ([Name]);

ALTER TABLE [Contestants] ADD CONSTRAINT [FK_Contestants_PositionApplications_PositionApplicationId] FOREIGN KEY ([PositionApplicationId]) REFERENCES [PositionApplications] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Positions] ADD CONSTRAINT [FK_Positions_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Positions] ADD CONSTRAINT [FK_Positions_Faculties_FacultyId] FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [RegisteredVoters] ADD CONSTRAINT [FK_RegisteredVoters_Elections_ElectionId] FOREIGN KEY ([ElectionId]) REFERENCES [Elections] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [RegisteredVoters] ADD CONSTRAINT [FK_RegisteredVoters_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Votes] ADD CONSTRAINT [FK_Votes_Contestants_ContestantId] FOREIGN KEY ([ContestantId]) REFERENCES [Contestants] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Votes] ADD CONSTRAINT [FK_Votes_ElectionPositions_ElectionPositionId] FOREIGN KEY ([ElectionPositionId]) REFERENCES [ElectionPositions] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Votes] ADD CONSTRAINT [FK_Votes_RegisteredVoters_RegisteredVoterId] FOREIGN KEY ([RegisteredVoterId]) REFERENCES [RegisteredVoters] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260921170025_RedesignElectionDomain', N'10.0.10');

COMMIT;
GO

