using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sys_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    secret_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_sys_role_permissions_sys_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "sys_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sys_role_permissions_sys_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "sys_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    expire_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_sys_user_roles_sys_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "sys_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sys_user_roles_sys_users_user_id",
                        column: x => x.user_id,
                        principalTable: "sys_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_sys_permissions_category",
                table: "sys_permissions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "idx_sys_permissions_code",
                table: "sys_permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_permissions_is_active",
                table: "sys_permissions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_sys_permissions_type",
                table: "sys_permissions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "idx_sys_role_permissions_permission_id",
                table: "sys_role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_sys_role_permissions_role_id",
                table: "sys_role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_sys_roles_is_active",
                table: "sys_roles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_sys_roles_is_default",
                table: "sys_roles",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "idx_sys_roles_name",
                table: "sys_roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_roles_priority",
                table: "sys_roles",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "idx_sys_user_roles_expire_time",
                table: "sys_user_roles",
                column: "expire_time",
                filter: "expire_time IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_sys_user_roles_is_active",
                table: "sys_user_roles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_sys_user_roles_role_id",
                table: "sys_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_sys_user_roles_user_id",
                table: "sys_user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_sys_user_roles_user_role",
                table: "sys_user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_account",
                table: "sys_users",
                column: "account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_creation_time",
                table: "sys_users",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_email",
                table: "sys_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_is_active",
                table: "sys_users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_phone_number",
                table: "sys_users",
                column: "phone_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sys_role_permissions");

            migrationBuilder.DropTable(
                name: "sys_user_roles");

            migrationBuilder.DropTable(
                name: "sys_permissions");

            migrationBuilder.DropTable(
                name: "sys_roles");

            migrationBuilder.DropTable(
                name: "sys_users");
        }
    }
}
