BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123124349_AddTwoFactorAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123124349_AddTwoFactorAuthentication'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorSecretProtected] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123124349_AddTwoFactorAuthentication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123124349_AddTwoFactorAuthentication', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123124754_AddTwoFactorRecoveryCodes'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorRecoveryCodeHashesJson] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123124754_AddTwoFactorRecoveryCodes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123124754_AddTwoFactorRecoveryCodes', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123125004_AddTwoFactorReplayProtection'
)
BEGIN
    ALTER TABLE [Users] ADD [TwoFactorLastUsedTimeStep] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123125004_AddTwoFactorReplayProtection'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123125004_AddTwoFactorReplayProtection', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123125212_AddLoginAttemptRetentionIndexes'
)
BEGIN
    CREATE INDEX [IX_LoginAttempts_LastFailedAttempt] ON [LoginAttempts] ([LastFailedAttempt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123125212_AddLoginAttemptRetentionIndexes'
)
BEGIN
    CREATE INDEX [IX_LoginAttemptOperations_CreatedAt] ON [LoginAttemptOperations] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261123125212_AddLoginAttemptRetentionIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123125212_AddLoginAttemptRetentionIndexes', N'10.0.11');
END;

COMMIT;
GO