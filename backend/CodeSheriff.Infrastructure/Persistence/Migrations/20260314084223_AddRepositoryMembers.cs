using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSheriff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositoryMembers : Migration
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

            migrationBuilder.CreateTable(
                name: "repository_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    invited_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    invite_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repository_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_repository_members_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_repositories_github_id_provider",
                table: "repositories",
                columns: new[] { "github_id", "git_provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_repository_members_clerk_user_id",
                table: "repository_members",
                column: "clerk_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_repository_members_invite_token",
                table: "repository_members",
                column: "invite_token",
                unique: true,
                filter: "invite_token IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_repository_members_repository_id_invited_email",
                table: "repository_members",
                columns: new[] { "repository_id", "invited_email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repository_members");

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
