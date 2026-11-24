using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace TaskTrackerAPI.Migrations
{
    [DbContext(typeof(TaskTrackerContext))]
    [Migration("20261123120000_UseValidatedEnums")]
    public partial class UseValidatedEnums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve existing meaning for the old zero-based task enum; normalize corrupt values.
            migrationBuilder.Sql("UPDATE [Tasks] SET [Status] = CASE [Status] WHEN 0 THEN 1 WHEN 1 THEN 2 WHEN 2 THEN 3 ELSE 1 END;");
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [RoleValue] int NOT NULL CONSTRAINT [DF_Users_RoleValue] DEFAULT 1;");
            migrationBuilder.Sql("UPDATE [Users] SET [RoleValue] = CASE WHEN LOWER(LTRIM(RTRIM([Role]))) = 'admin' THEN 2 ELSE 1 END;");
            migrationBuilder.DropColumn(name: "Role", table: "Users");
            migrationBuilder.RenameColumn(name: "RoleValue", table: "Users", newName: "Role");
            migrationBuilder.AddCheckConstraint(name: "CK_Tasks_Status", table: "Tasks", sql: "[Status] IN (1, 2, 3)");
            migrationBuilder.AddCheckConstraint(name: "CK_Users_Role", table: "Users", sql: "[Role] IN (1, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Tasks_Status", table: "Tasks");
            migrationBuilder.DropCheckConstraint(name: "CK_Users_Role", table: "Users");
            migrationBuilder.Sql("ALTER TABLE [Users] ADD [RoleText] nvarchar(50) NOT NULL CONSTRAINT [DF_Users_RoleText] DEFAULT N'User';");
            migrationBuilder.Sql("UPDATE [Users] SET [RoleText] = CASE [Role] WHEN 2 THEN N'Admin' ELSE N'User' END;");
            migrationBuilder.DropColumn(name: "Role", table: "Users");
            migrationBuilder.RenameColumn(name: "RoleText", table: "Users", newName: "Role");
            migrationBuilder.Sql("UPDATE [Tasks] SET [Status] = CASE [Status] WHEN 1 THEN 0 WHEN 2 THEN 1 ELSE 2 END;");
        }
    }
}