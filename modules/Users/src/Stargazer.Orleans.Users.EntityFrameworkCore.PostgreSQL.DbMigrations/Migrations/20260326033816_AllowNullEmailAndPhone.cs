using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullEmailAndPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sys_users_email",
                table: "sys_users");

            migrationBuilder.DropIndex(
                name: "idx_sys_users_phone_number",
                table: "sys_users");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "sys_users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "sys_users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_email",
                table: "sys_users",
                column: "email",
                unique: true,
                filter: "(email IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_phone_number",
                table: "sys_users",
                column: "phone_number",
                unique: true,
                filter: "(phone_number IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_sys_users_email",
                table: "sys_users");

            migrationBuilder.DropIndex(
                name: "idx_sys_users_phone_number",
                table: "sys_users");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "sys_users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "sys_users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_email",
                table: "sys_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sys_users_phone_number",
                table: "sys_users",
                column: "phone_number",
                unique: true);
        }
    }
}
