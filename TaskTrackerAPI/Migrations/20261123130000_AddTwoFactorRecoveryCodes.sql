BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [TwoFactorRecoveryCodeHashesJson] nvarchar(2000) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261123124754_AddTwoFactorRecoveryCodes', N'10.0.11');

COMMIT;
GO