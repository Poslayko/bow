using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramAccountState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "state",
                table: "telegram_accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "state",
                table: "telegram_accounts");
        }
    }
}
