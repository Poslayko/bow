using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueFilterForUserVocabularyStatusAndResetStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE user_vocabulary_progresses
                SET status = 0;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_user_vocabulary_progresses_waiting_answer",
                table: "user_vocabulary_progresses",
                column: "user_id",
                unique: true,
                filter: "status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_user_vocabulary_progresses_waiting_answer",
                table: "user_vocabulary_progresses");
        }
    }
}
