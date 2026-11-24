BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    UPDATE [Tasks] SET [Status] = CASE [Status] WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 3 ELSE 1 END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    ALTER TABLE [Users] ADD [RoleValue] int NOT NULL CONSTRAINT [DF_Users_RoleValue] DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    UPDATE [Users] SET [RoleValue] = CASE WHEN LOWER(LTRIM(RTRIM([Role]))) = 'admin' THEN 2 ELSE 1 END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Users] DROP COLUMN [Role];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    EXEC sp_rename N'[Users].[RoleValue]', N'Role', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    EXEC(N'ALTER TABLE [Tasks] ADD CONSTRAINT [CK_Tasks_Status] CHECK ([Status] IN (1, 2, 3))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    EXEC(N'ALTER TABLE [Users] ADD CONSTRAINT [CK_Users_Role] CHECK ([Role] IN (1, 2))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123120000_UseValidatedEnums'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123120000_UseValidatedEnums', N'10.0.11');
END;

COMMIT;
GO