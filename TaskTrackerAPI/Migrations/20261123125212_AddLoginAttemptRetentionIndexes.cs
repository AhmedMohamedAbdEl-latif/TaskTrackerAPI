using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginAttemptRetentionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_LastFailedAttempt",
                table: "LoginAttempts",
                column: "LastFailedAttempt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttemptOperations_CreatedAt",
                table: "LoginAttemptOperations",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoginAttempts_LastFailedAttempt",
                table: "LoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttemptOperations_CreatedAt",
                table: "LoginAttemptOperations");
        }
    }
}
