BEGIN TRANSACTION;
DROP INDEX [IX_Departments_FacultyId] ON [Departments];

DROP INDEX [EmailIndex] ON [AspNetUsers];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserTypes]') AND [c].[name] = N'Name');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [UserTypes] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [UserTypes] ALTER COLUMN [Name] nvarchar(50) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Students]') AND [c].[name] = N'RegNumber');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Students] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Students] ALTER COLUMN [RegNumber] nvarchar(50) NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Gender]') AND [c].[name] = N'Name');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Gender] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Gender] ALTER COLUMN [Name] nvarchar(50) NOT NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Faculties]') AND [c].[name] = N'Name');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Faculties] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Faculties] ALTER COLUMN [Name] nvarchar(150) NOT NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Departments]') AND [c].[name] = N'Name');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Departments] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Departments] ALTER COLUMN [Name] nvarchar(150) NOT NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'StreetName');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Addresses] ALTER COLUMN [StreetName] nvarchar(200) NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'State');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Addresses] ALTER COLUMN [State] nvarchar(100) NULL;

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'Nationality');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [Addresses] ALTER COLUMN [Nationality] nvarchar(100) NULL;

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'City');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [Addresses] ALTER COLUMN [City] nvarchar(100) NULL;

ALTER TABLE [Addresses] ADD [UserId] nvarchar(450) NULL;


        UPDATE a
        SET a.UserId = s.UserId
        FROM Addresses a
        INNER JOIN Students s ON a.StudentId = s.Id
        WHERE a.StudentId IS NOT NULL;
    


        UPDATE a
        SET a.UserId = st.UserId
        FROM Addresses a
        INNER JOIN StaffProfile st ON a.StaffId = st.Id
        WHERE a.StaffId IS NOT NULL;
    


        IF EXISTS (
            SELECT 1
            FROM Addresses
            WHERE UserId IS NULL
        )
        BEGIN
            THROW 50001, 'Address migration failed because one or more addresses could not be mapped to a user.', 1;
        END
    

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'UserId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [Addresses] ALTER COLUMN [UserId] nvarchar(450) NOT NULL;

ALTER TABLE [Addresses] DROP CONSTRAINT [FK_Addresses_StaffProfile_StaffId];

ALTER TABLE [Addresses] DROP CONSTRAINT [FK_Addresses_Students_StudentId];

DROP INDEX [IX_Addresses_StaffId] ON [Addresses];

DROP INDEX [IX_Addresses_StudentId] ON [Addresses];

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'StaffId');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [Addresses] DROP COLUMN [StaffId];

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Addresses]') AND [c].[name] = N'StudentId');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Addresses] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [Addresses] DROP COLUMN [StudentId];

CREATE UNIQUE INDEX [IX_UserTypes_Name] ON [UserTypes] ([Name]);

CREATE UNIQUE INDEX [IX_Students_RegNumber] ON [Students] ([RegNumber]) WHERE [RegNumber] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Gender_Name] ON [Gender] ([Name]);

CREATE UNIQUE INDEX [IX_Faculties_Name] ON [Faculties] ([Name]);

CREATE UNIQUE INDEX [IX_Departments_FacultyId_Name] ON [Departments] ([FacultyId], [Name]);

CREATE UNIQUE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]) WHERE [NormalizedEmail] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Addresses_UserId] ON [Addresses] ([UserId]);

ALTER TABLE [Addresses] ADD CONSTRAINT [FK_Addresses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260921124829_RefactorAddressAndAddUniqueConstraints', N'10.0.10');

COMMIT;
GO

