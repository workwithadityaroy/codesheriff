using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeSheriff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ai_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ai_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ai_api_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    notification_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    weekly_report_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_settings_clerk_user_id",
                table: "user_settings",
                column: "clerk_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_settings");
        }
    }
}
