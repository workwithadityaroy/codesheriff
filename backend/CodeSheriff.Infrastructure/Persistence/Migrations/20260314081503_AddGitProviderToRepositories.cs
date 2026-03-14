using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSheriff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGitProviderToRepositories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_repositories_github_id",
                table: "repositories");

            migrationBuilder.AddColumn<string>(
                name: "access_token",
                table: "repositories",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "git_provider",
                table: "repositories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "GitHub");

            migrationBuilder.CreateIndex(
                name: "ix_repositories_github_id_provider",
                table: "repositories",
                columns: new[] { "github_id", "git_provider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_repositories_github_id_provider",
                table: "repositories");

            migrationBuilder.DropColumn(
                name: "access_token",
                table: "repositories");

            migrationBuilder.DropColumn(
                name: "git_provider",
                table: "repositories");

            migrationBuilder.CreateIndex(
                name: "ix_repositories_github_id",
                table: "repositories",
                column: "github_id",
                unique: true);
        }
    }
}
