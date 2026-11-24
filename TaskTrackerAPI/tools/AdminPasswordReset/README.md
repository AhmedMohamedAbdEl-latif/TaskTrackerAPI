# Admin password reset utility

Run this local console utility from the API project directory:

```powershell
dotnet run --project tools/AdminPasswordReset/AdminPasswordReset.csproj
```

It reads `ConnectionStrings:TaskTrackerDatabase` from the API's `appsettings.json`, the matching `appsettings.{ASPNETCORE_ENVIRONMENT}.json`, or environment variables. It prompts for the exact Admin email, requires typing `RESET`, and reads the new password without displaying it. The password must be 12–128 characters.

The utility updates only that Admin's password hash using the same ASP.NET Core `PasswordHasher<User>` as the API. It preserves the user's role, two-factor setup, and all related records. It does not print the connection string, password, or password hash. Do not commit database credentials to source control.
