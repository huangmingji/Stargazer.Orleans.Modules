using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWechatManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wechat_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    app_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    app_secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    encoding_aes_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    access_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    access_token_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wechat_message_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    open_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    template_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    send_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    complete_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    msg_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    extra_data = table.Column<string>(type: "text", nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_message_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_wechat_message_logs_wechat_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "wechat_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wechat_user_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    wechat_group_id = table.Column<int>(type: "integer", nullable: false),
                    user_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_user_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_wechat_user_groups_wechat_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "wechat_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wechat_user_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    wechat_tag_id = table.Column<int>(type: "integer", nullable: false),
                    user_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_user_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_wechat_user_tags_wechat_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "wechat_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wechat_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    open_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    union_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nickname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sex = table.Column<int>(type: "integer", nullable: false),
                    province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    city = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    headimg_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    subscribe_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    unsubscribe_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subscribe_status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modify_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_wechat_users_wechat_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "wechat_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wechat_users_wechat_user_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "wechat_user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WechatUserWechatUserTag",
                columns: table => new
                {
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WechatUserWechatUserTag", x => new { x.TagsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_WechatUserWechatUserTag_wechat_user_tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "wechat_user_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WechatUserWechatUserTag_wechat_users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "wechat_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wechat_accounts_app_id",
                table: "wechat_accounts",
                column: "app_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_accounts_is_active",
                table: "wechat_accounts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_accounts_is_default",
                table: "wechat_accounts",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_message_logs_account_id",
                table: "wechat_message_logs",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_message_logs_creation_time",
                table: "wechat_message_logs",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_message_logs_open_id",
                table: "wechat_message_logs",
                column: "open_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_message_logs_status",
                table: "wechat_message_logs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_groups_account_id_name",
                table: "wechat_user_groups",
                columns: new[] { "account_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_tags_account_id_name",
                table: "wechat_user_tags",
                columns: new[] { "account_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_users_account_id",
                table: "wechat_users",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_users_account_id_open_id",
                table: "wechat_users",
                columns: new[] { "account_id", "open_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_users_group_id",
                table: "wechat_users",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_users_open_id",
                table: "wechat_users",
                column: "open_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_users_subscribe_status",
                table: "wechat_users",
                column: "subscribe_status");

            migrationBuilder.CreateIndex(
                name: "IX_WechatUserWechatUserTag_UsersId",
                table: "WechatUserWechatUserTag",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wechat_message_logs");

            migrationBuilder.DropTable(
                name: "WechatUserWechatUserTag");

            migrationBuilder.DropTable(
                name: "wechat_user_tags");

            migrationBuilder.DropTable(
                name: "wechat_users");

            migrationBuilder.DropTable(
                name: "wechat_user_groups");

            migrationBuilder.DropTable(
                name: "wechat_accounts");
        }
    }
}
