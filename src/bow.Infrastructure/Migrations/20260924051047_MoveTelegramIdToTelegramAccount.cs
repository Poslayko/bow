using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveTelegramIdToTelegramAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_telegram_id",
                table: "users");

            migrationBuilder.CreateTable(
                name: "telegram_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_telegram_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO telegram_accounts (user_id, telegram_id)
                SELECT id, telegram_id
                FROM users
                WHERE telegram_id IS NOT NULL;
            """);

            migrationBuilder.DropColumn(
                name: "telegram_id",
                table: "users");

            migrationBuilder.CreateIndex(
                name: "ix_telegram_accounts_telegram_id",
                table: "telegram_accounts",
                column: "telegram_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telegram_accounts_user_id",
                table: "telegram_accounts",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "telegram_id",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("""
                UPDATE users
                SET telegram_id = telegram_accounts.telegram_id
                FROM telegram_accounts
                WHERE telegram_accounts.user_id = users.id;
            """);

            migrationBuilder.DropTable(
                name: "telegram_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_users_telegram_id",
                table: "users",
                column: "telegram_id",
                unique: true);
        }
    }
}
