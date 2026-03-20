using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateMessageManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "msg_provider_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    config_json = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    max_qps = table.Column<int>(type: "integer", nullable: false),
                    current_qps = table.Column<int>(type: "integer", nullable: false),
                    is_healthy = table.Column<bool>(type: "boolean", nullable: false),
                    last_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msg_provider_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "msg_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    template_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    receiver = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    variables = table.Column<string>(type: "jsonb", nullable: true),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: true),
                    business_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    business_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msg_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "msg_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    subject_template = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content_template = table.Column<string>(type: "text", nullable: false),
                    variables = table.Column<string>(type: "jsonb", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    default_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msg_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_msg_provider_configs_channel",
                table: "msg_provider_configs",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "idx_msg_provider_configs_is_enabled",
                table: "msg_provider_configs",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "idx_msg_records_channel",
                table: "msg_records",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "idx_msg_records_creation_time",
                table: "msg_records",
                column: "creation_time",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_msg_records_receiver",
                table: "msg_records",
                column: "receiver");

            migrationBuilder.CreateIndex(
                name: "idx_msg_records_scheduled_at",
                table: "msg_records",
                column: "scheduled_at",
                filter: "scheduled_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_msg_records_status",
                table: "msg_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_msg_templates_channel",
                table: "msg_templates",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "idx_msg_templates_is_active",
                table: "msg_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_msg_templates_code_channel",
                table: "msg_templates",
                columns: new[] { "code", "channel" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "msg_provider_configs");

            migrationBuilder.DropTable(
                name: "msg_records");

            migrationBuilder.DropTable(
                name: "msg_templates");
        }
    }
}
