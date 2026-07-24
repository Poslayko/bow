using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bow.Infrastructure.Migrations
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
                name: "vocabulary_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    language = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vocabulary_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_vocabulary_progresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    vocabulary_item_id = table.Column<int>(type: "integer", nullable: false),
                    stage = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    next_review_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_vocabulary_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_vocabulary_progresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_vocabulary_progresses_vocabulary_items_vocabulary_item",
                        column: x => x.vocabulary_item_id,
                        principalTable: "vocabulary_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_translations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    translation_from_id = table.Column<int>(type: "integer", nullable: false),
                    translation_to_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vocabulary_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_vocabulary_translations_vocabulary_items_translation_from_id",
                        column: x => x.translation_from_id,
                        principalTable: "vocabulary_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vocabulary_translations_vocabulary_items_translation_to_id",
                        column: x => x.translation_to_id,
                        principalTable: "vocabulary_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_vocabulary_progresses_user_id_next_review_at",
                table: "user_vocabulary_progresses",
                columns: new[] { "user_id", "next_review_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_vocabulary_progresses_user_id_vocabulary_item_id",
                table: "user_vocabulary_progresses",
                columns: new[] { "user_id", "vocabulary_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_vocabulary_progresses_vocabulary_item_id",
                table: "user_vocabulary_progresses",
                column: "vocabulary_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_telegram_id",
                table: "users",
                column: "telegram_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_items_language_normalized_text",
                table: "vocabulary_items",
                columns: new[] { "language", "normalized_text" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_translations_translation_from_id_translation_to_",
                table: "vocabulary_translations",
                columns: new[] { "translation_from_id", "translation_to_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_translations_translation_to_id",
                table: "vocabulary_translations",
                column: "translation_to_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_vocabulary_progresses");

            migrationBuilder.DropTable(
                name: "vocabulary_translations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "vocabulary_items");
        }
    }
}
