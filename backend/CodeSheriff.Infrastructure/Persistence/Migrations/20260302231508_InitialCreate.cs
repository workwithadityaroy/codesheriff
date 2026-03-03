using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSheriff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "repositories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    github_id = table.Column<long>(type: "bigint", nullable: false),
                    owner = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "character varying(511)", maxLength: 511, nullable: false),
                    installation_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repositories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pull_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    github_pr_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    head_branch = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    base_branch = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    author_login = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pull_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_pull_requests_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    average_tech_debt_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    total_reviews = table.Column<int>(type: "integer", nullable: false),
                    total_issues = table.Column<int>(type: "integer", nullable: false),
                    summary = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_weekly_reports_repositories_repository_id",
                        column: x => x.repository_id,
                        principalTable: "repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pull_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tech_debt_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    summary = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_ai_response = table.Column<string>(type: "text", nullable: true),
                    processing_time_ms = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_pull_requests_pull_request_id",
                        column: x => x.pull_request_id,
                        principalTable: "pull_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_issues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    suggestion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_issues", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_issues_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pull_requests_repository_pr_number",
                table: "pull_requests",
                columns: new[] { "repository_id", "github_pr_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_repositories_full_name",
                table: "repositories",
                column: "full_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_repositories_github_id",
                table: "repositories",
                column: "github_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_review_issues_review_id",
                table: "review_issues",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "ix_review_issues_severity",
                table: "review_issues",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_pull_request_id",
                table: "reviews",
                column: "pull_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_weekly_reports_repository_id",
                table: "weekly_reports",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "ix_weekly_reports_repository_period",
                table: "weekly_reports",
                columns: new[] { "repository_id", "period_start" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "review_issues");

            migrationBuilder.DropTable(
                name: "weekly_reports");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "pull_requests");

            migrationBuilder.DropTable(
                name: "repositories");
        }
    }
}
