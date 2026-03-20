using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Stargazer.Orleans.ObjectStorage.Domain.Entities;

#nullable disable

namespace Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL.DbMigrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateObjectStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "os_bucket_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Principal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Resource = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Actions = table.Column<string>(type: "jsonb", nullable: false),
                    Effect = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PolicyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_bucket_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_buckets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Acl = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxObjectSize = table.Column<long>(type: "bigint", nullable: false),
                    MaxObjectCount = table.Column<long>(type: "bigint", nullable: false),
                    CurrentObjectCount = table.Column<long>(type: "bigint", nullable: false),
                    CurrentStorageSize = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_buckets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_multipart_uploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    UploadId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    TotalSize = table.Column<long>(type: "bigint", nullable: false),
                    PartSize = table.Column<int>(type: "integer", nullable: false),
                    TotalParts = table.Column<int>(type: "integer", nullable: false),
                    UploadedParts = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    InitiatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Parts = table.Column<List<UploadPart>>(type: "jsonb", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_multipart_uploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_objects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BucketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ETag = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CacheControl = table.Column<string>(type: "text", nullable: false),
                    ContentDisposition = table.Column<string>(type: "text", nullable: false),
                    ContentEncoding = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifyTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_objects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_os_bucket_policies_BucketId_Principal",
                table: "os_bucket_policies",
                columns: new[] { "BucketId", "Principal" });

            migrationBuilder.CreateIndex(
                name: "IX_os_buckets_Name",
                table: "os_buckets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_os_buckets_OwnerId",
                table: "os_buckets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_os_multipart_uploads_BucketId_Key_UploadId",
                table: "os_multipart_uploads",
                columns: new[] { "BucketId", "Key", "UploadId" });

            migrationBuilder.CreateIndex(
                name: "IX_os_multipart_uploads_UploadId",
                table: "os_multipart_uploads",
                column: "UploadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_os_objects_BucketId",
                table: "os_objects",
                column: "BucketId");

            migrationBuilder.CreateIndex(
                name: "IX_os_objects_BucketId_Key",
                table: "os_objects",
                columns: new[] { "BucketId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "os_bucket_policies");

            migrationBuilder.DropTable(
                name: "os_buckets");

            migrationBuilder.DropTable(
                name: "os_multipart_uploads");

            migrationBuilder.DropTable(
                name: "os_objects");
        }
    }
}
