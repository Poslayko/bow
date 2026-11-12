using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserVocabularyProgressStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "user_vocabulary_progresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "user_vocabulary_progresses");
        }
    }
}
