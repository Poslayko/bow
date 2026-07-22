using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    native_language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    learning_language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    learning_level = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "words",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    level = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_words", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_word_progresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    word_id = table.Column<int>(type: "integer", nullable: false),
                    stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    next_review_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_word_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_word_progresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_word_progresses_words_word_id",
                        column: x => x.word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "word_translations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_word_id = table.Column<int>(type: "integer", nullable: false),
                    target_word_id = table.Column<int>(type: "integer", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_word_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_word_translations_words_source_word_id",
                        column: x => x.source_word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_word_translations_words_target_word_id",
                        column: x => x.target_word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_word_progresses_user_id_next_review_at",
                table: "user_word_progresses",
                columns: new[] { "user_id", "next_review_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_word_progresses_user_id_word_id",
                table: "user_word_progresses",
                columns: new[] { "user_id", "word_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_word_progresses_word_id",
                table: "user_word_progresses",
                column: "word_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_telegram_id",
                table: "users",
                column: "telegram_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_word_translations_source_word_id_target_word_id",
                table: "word_translations",
                columns: new[] { "source_word_id", "target_word_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_word_translations_target_word_id",
                table: "word_translations",
                column: "target_word_id");

            migrationBuilder.CreateIndex(
                name: "ix_words_language_level_text",
                table: "words",
                columns: new[] { "language", "level", "text" });

            migrationBuilder.CreateIndex(
                name: "ix_words_text",
                table: "words",
                column: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_word_progresses");

            migrationBuilder.DropTable(
                name: "word_translations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "words");
        }
    }
}
