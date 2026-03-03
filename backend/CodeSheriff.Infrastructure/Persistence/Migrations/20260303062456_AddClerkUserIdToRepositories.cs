using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSheriff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClerkUserIdToRepositories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "clerk_user_id",
                table: "repositories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_repositories_clerk_user_id",
                table: "repositories",
                column: "clerk_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_repositories_clerk_user_id",
                table: "repositories");

            migrationBuilder.DropColumn(
                name: "clerk_user_id",
                table: "repositories");
        }
    }
}
