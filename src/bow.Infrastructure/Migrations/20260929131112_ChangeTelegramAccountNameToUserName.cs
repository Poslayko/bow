using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTelegramAccountNameToUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "telegram_accounts",
                newName: "user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "telegram_accounts",
                newName: "name");
        }
    }
}
