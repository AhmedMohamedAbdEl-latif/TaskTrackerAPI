BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Users] ADD [TwoFactorSecretProtected] nvarchar(2000) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261123124349_AddTwoFactorAuthentication', N'10.0.11');

COMMIT;
GO