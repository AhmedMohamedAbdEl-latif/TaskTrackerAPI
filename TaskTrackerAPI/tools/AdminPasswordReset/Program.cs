using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using TaskTrackerAPI.Models;

try
{
    var apiDirectory = FindApiDirectory(Directory.GetCurrentDirectory());
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    var configuration = new ConfigurationBuilder()
        .SetBasePath(apiDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration.GetConnectionString("TaskTrackerDatabase");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:TaskTrackerDatabase was not found in the API configuration or environment variables.");

    Console.WriteLine("Admin password reset utility");
    Console.WriteLine("This updates only the password hash for one existing Admin. Other account data is preserved.");
    Console.Write("Admin email: ");
    var email = Console.ReadLine()?.Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email))
        throw new InvalidOperationException("An email address is required.");

    var options = new DbContextOptionsBuilder<TaskTrackerContext>()
        .UseSqlServer(connectionString)
        .Options;
    await using var db = new TaskTrackerContext(options);
    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

    var admin = await db.Users.SingleOrDefaultAsync(
        user => user.Email == email && user.Role == UserRole.Admin);
    if (admin is null)
        throw new InvalidOperationException("No Admin account with that exact email was found. No data was changed.");

    Console.WriteLine($"Target account: {admin.Email} (ID {admin.Id})");
    Console.Write("Type RESET to confirm: ");
    if (!string.Equals(Console.ReadLine(), "RESET", StringComparison.Ordinal))
        throw new OperationCanceledException("Confirmation did not match. No data was changed.");

    var password = ReadHidden("New password (minimum 12 characters): ");
    var confirmation = ReadHidden("Repeat new password: ");
    if (!string.Equals(password, confirmation, StringComparison.Ordinal))
        throw new InvalidOperationException("The passwords do not match. No data was changed.");
    if (password.Length < 12 || password.Length > 128)
        throw new InvalidOperationException("Password must be between 12 and 128 characters. No data was changed.");

    admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    Console.WriteLine("Password reset completed. Two-factor settings and all related records were preserved.");
    return 0;
}
catch (OperationCanceledException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Password reset failed: {exception.Message}");
    return 1;
}

static string FindApiDirectory(string startDirectory)
{
    var current = new DirectoryInfo(startDirectory);
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "TaskTrackerAPI.csproj")))
            return current.FullName;
        current = current.Parent;
    }

    throw new DirectoryNotFoundException(
        "Could not locate TaskTrackerAPI.csproj. Run this utility from the API project folder or one of its child folders.");
}

static string ReadHidden(string prompt)
{
    Console.Write(prompt);
    var value = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return value.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (value.Length > 0)
            {
                value.Length--;
                Console.Write("\b \b");
            }
            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            value.Append(key.KeyChar);
            Console.Write('*');
        }
    }
}
