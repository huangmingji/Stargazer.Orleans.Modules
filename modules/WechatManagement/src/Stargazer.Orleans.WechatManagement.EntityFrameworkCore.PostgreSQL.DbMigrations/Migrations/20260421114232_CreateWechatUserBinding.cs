using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class CreateWechatUserBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wechat_user_bindings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wechat_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    open_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    binding_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wechat_user_bindings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_bindings_account_id",
                table: "wechat_user_bindings",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_bindings_account_id_open_id",
                table: "wechat_user_bindings",
                columns: new[] { "account_id", "open_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_bindings_local_user_id",
                table: "wechat_user_bindings",
                column: "local_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wechat_user_bindings_open_id",
                table: "wechat_user_bindings",
                column: "open_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wechat_user_bindings");
        }
    }
}
