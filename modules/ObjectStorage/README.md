# ObjectStorage 模块

基于 Orleans 框架的分布式对象存储模块，提供存储桶管理、对象存储、分片上传、签名 URL 等功能。

## 功能特性

### 存储桶管理
- 创建、查询、更新、删除存储桶
- 存储桶访问控制 (ACL: Private/PublicRead/PublicReadWrite)
- 存储桶策略 (Bucket Policy) 细粒度权限控制
- 用户存储桶列表查询

### 对象存储
- 对象上传、下载、删除
- 对象元数据管理
- 对象列表查询 (支持前缀过滤和分页)
- 对象存在性检查

### 高级功能
- **签名 URL**: 生成临时访问链接，支持自定义过期时间 (最长 7 天)
- **分片上传**: 大文件分片上传，支持断点续传
- **权限继承**: 基于存储桶 ACL 和用户角色自动授权

### 权限系统
- 基于 Users 模块的权限体系
- 8 个存储桶/对象操作权限
- ObjectStorageAdmin 预定义角色
- Silo 启动时自动初始化种子数据

## 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  ┌──────────────────┐    ┌─────────────────────────────┐    │
│  │ BucketController │    │    ObjectController         │    │
│  │  /api/storage/   │    │    /api/storage/object      │    │
│  └────────┬─────────┘    └──────────────┬──────────────┘    │
└───────────┼─────────────────────────────┼───────────────────┘
            │                             │
            ▼                             ▼
┌─────────────────────────────────────────────────────────────┐
│                      Grain Layer                            │
│  ┌──────────────────┐    ┌─────────────────────────────┐  │
│  │   BucketGrain    │    │      ObjectGrain            │  │
│  │  - CRUD Bucket   │    │  - Upload/Download          │  │
│  │  - ACL Check     │    │  - Multipart Upload         │  │
│  │  - Policy Eval   │    │  - Signed URL               │  │
│  └────────┬─────────┘    └─────────────┬───────────────┘  │
└───────────┼─────────────────────────────┼──────────────────┘
            │                             │
            ▼                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    Storage Provider Layer                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │  Local   │ │ Aliyun   │ │   AWS    │ │  Azure   │       │
│  │   OSS    │ │   S3     │ │  Blob    │ │   COS    │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
└─────────────────────────────────────────────────────────────┘
```

## 快速开始

### 1. 数据库配置

在 `appsettings.json` 中配置连接字符串：

```json
{
  "ConnectionStrings": {
    "ObjectStorage": "server=127.0.0.1;port=5432;Database=objectstorage;uid=postgres;pwd=123456",
    "Redis": "127.0.0.1:6379"
  }
}
```

确保 PostgreSQL 中已创建 `objectstorage` 数据库：

```sql
CREATE DATABASE objectstorage;
```

### 2. 配置存储提供者

在 `appsettings.json` 中配置 Storage 部分：

```json
{
  "Storage": {
    "Provider": "local",
    "DefaultExpiryMinutes": 60,
    "Local": {
      "BasePath": "/data/objects"
    }
  }
}
```

### 3. JWT 配置

确保正确配置 JWT 设置：

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "Stargazer.Orleans",
    "Audience": "Stargazer.Orleans",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryMinutes": 10080
  }
}
```

### 4. 种子数据

Silo 启动时自动初始化以下数据：

- **8 个权限**: `storage.bucket.view`, `storage.bucket.create`, `storage.bucket.update`, `storage.bucket.delete`, `storage.object.view`, `storage.object.create`, `storage.object.update`, `storage.object.delete`
- **ObjectStorageAdmin 角色**: 拥有所有存储权限
- **角色分配**: admin 用户自动获得 ObjectStorageAdmin 角色

## 权限策略

存储模块使用 Users 模块的权限系统，所有 API 需要 JWT 认证和权限检查：

| 权限代码 | 名称 | 描述 |
|---------|------|------|
| `storage.bucket.view` | 查看存储桶 | 查看存储桶列表和详情 |
| `storage.bucket.create` | 创建存储桶 | 创建新存储桶 |
| `storage.bucket.update` | 更新存储桶 | 编辑存储桶信息 |
| `storage.bucket.delete` | 删除存储桶 | 删除存储桶 |
| `storage.object.view` | 查看对象 | 查看和下载对象 |
| `storage.object.create` | 上传对象 | 上传新对象 |
| `storage.object.update` | 更新对象 | 更新对象内容 |
| `storage.object.delete` | 删除对象 | 删除对象 |

## API 接口

### 认证

所有 API 端点都需要有效的 JWT Token：

```
Authorization: Bearer <token>
```

### 存储桶接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/storage/bucket/{id}` | 获取存储桶 | storage.bucket.view |
| GET | `/api/storage/bucket/name/{name}` | 按名称获取存储桶 | storage.bucket.view |
| GET | `/api/storage/bucket` | 获取当前用户的所有存储桶 | storage.bucket.view |
| POST | `/api/storage/bucket` | 创建存储桶 | storage.bucket.create |
| PUT | `/api/storage/bucket/{id}` | 更新存储桶 | storage.bucket.update |
| DELETE | `/api/storage/bucket/{id}` | 删除存储桶 | storage.bucket.delete |
| GET | `/api/storage/bucket/{id}/access` | 检查访问权限 | storage.bucket.view |

### 对象接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/storage/object/{bucketId}/{key}` | 下载对象 | storage.object.view |
| HEAD | `/api/storage/object/{bucketId}/{key}` | 检查对象是否存在 | storage.object.view |
| GET | `/api/storage/object/metadata/{bucketId}/{key}` | 获取对象元数据 | storage.object.view |
| GET | `/api/storage/object/{bucketId}` | 列出对象 (分页) | storage.object.view |
| POST | `/api/storage/object/{bucketId}/{key}` | 上传对象 | storage.object.create |
| PUT | `/api/storage/object/{bucketId}/{key}` | 更新对象 | storage.object.update |
| DELETE | `/api/storage/object/{bucketId}/{key}` | 删除对象 | storage.object.delete |
| GET | `/api/storage/object/signed-url/{bucketId}/{key}` | 获取签名URL | storage.object.view |
| POST | `/api/storage/object/multipart/initiate/{bucketId}/{key}` | 初始化分片上传 | storage.object.create |
| POST | `/api/storage/object/multipart/part/{bucketId}/{uploadId}/{key}` | 上传分片 | storage.object.create |
| POST | `/api/storage/object/multipart/complete/{bucketId}/{uploadId}/{key}` | 完成分片上传 | storage.object.create |
| DELETE | `/api/storage/object/multipart/{bucketId}/{uploadId}/{key}` | 取消分片上传 | storage.object.delete |

### 请求示例

#### 创建存储桶
```bash
curl -X POST http://localhost:5000/api/storage/bucket \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "my-bucket",
    "description": "My first bucket",
    "acl": "Private",
    "maxObjectSize": 104857600,
    "maxObjectCount": 1000
  }'
```

#### 上传对象
```bash
curl -X POST http://localhost:5000/api/storage/object/{bucketId}/my-file.txt \
  -H "Authorization: Bearer <token>" \
  -F "file=@my-file.txt" \
  -F "contentType=text/plain"
```

#### 获取签名 URL
```bash
curl -X GET "http://localhost:5000/api/storage/object/signed-url/{bucketId}/my-file.txt?expiry=01:00:00&method=GET" \
  -H "Authorization: Bearer <token>"
```

## 云存储提供者配置

### 本地存储 (Local)
```json
{
  "Provider": "local",
  "Local": {
    "BasePath": "/data/objects"
  }
}
```

### 阿里云 OSS
```json
{
  "Provider": "aliyun",
  "Aliyun": {
    "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
    "AccessKeyId": "your-access-key",
    "AccessKeySecret": "your-secret",
    "BucketName": "your-bucket"
  }
}
```

### AWS S3
```json
{
  "Provider": "aws",
  "Aws": {
    "Region": "us-east-1",
    "AccessKeyId": "your-access-key",
    "SecretAccessKey": "your-secret",
    "BucketName": "your-bucket"
  }
}
```

### Azure Blob
```json
{
  "Provider": "azure",
  "Azure": {
    "ConnectionString": "your-connection-string",
    "ContainerName": "your-container"
  }
}
```

### 腾讯云 COS
```json
{
  "Provider": "tencent",
  "Tencent": {
    "Region": "ap-guangzhou",
    "SecretId": "your-secret-id",
    "SecretKey": "your-secret-key",
    "BucketName": "your-bucket"
  }
}
```

### MinIO
```json
{
  "Provider": "minio",
  "Minio": {
    "Endpoint": "localhost:9000",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "BucketName": "your-bucket",
    "UseSSL": false
  }
}
```

**注意**: 所有存储提供者均已实现，包括 `LocalStorageProvider`、`AliyunOssProvider`、`AwsS3Provider`、`AzureBlobProvider`、`TencentCosProvider` 和 `MinioProvider`。

## 数据模型

### Bucket (存储桶)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| Name | string | 存储桶名称 (唯一) |
| Description | string | 描述 |
| Acl | BucketAclType | 访问控制 (Private/PublicRead/PublicReadWrite) |
| MaxObjectSize | long | 最大对象大小 (字节) |
| MaxObjectCount | int | 最大对象数量 |
| CurrentObjectCount | int | 当前对象数量 |
| CurrentStorageSize | long | 当前存储大小 (字节) |
| OwnerId | GUID | 所有者 ID |
| IsActive | bool | 是否激活 |
| CreationTime | DateTime | 创建时间 |
| LastModifyTime | DateTime | 最后修改时间 |

### ObjectInfo (对象)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| BucketId | GUID | 存储桶 ID |
| Key | string | 对象键 |
| FileName | string? | 文件名 |
| ContentType | string | Content-Type |
| Size | long | 大小 (字节) |
| ETag | string | ETag |
| Metadata | string | 元数据 (JSON) |
| IsDeleted | bool | 是否已删除 |
| LastModified | DateTime | 最后修改时间 |
| CreationTime | DateTime | 创建时间 |

### MultipartUpload (分片上传)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| BucketId | GUID | 存储桶 ID |
| Key | string | 对象键 |
| UploadId | string | 上传 ID |
| ContentType | string | Content-Type |
| Metadata | string | 元数据 |
| UploadedParts | int | 已上传分片数 |
| Status | UploadStatus | 状态 |
| ExpiresAt | DateTime | 过期时间 |
| CreationTime | DateTime | 创建时间 |

### BucketPolicy (存储桶策略)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| BucketId | GUID | 存储桶 ID |
| Principal | string | 授权主体 (用户/角色) |
| Resource | string | 资源路径 |
| Actions | List<string> | 操作列表 |
| Effect | EffectType | 效果 (Allow/Deny) |
| IsActive | bool | 是否激活 |

## 与 Users 模块集成

ObjectStorage 模块依赖 Users 模块进行身份认证和权限检查：

1. **JWT 认证**: 所有 API 端点都需要有效的 JWT Token
2. **权限检查**: 基于存储桶 ACL 和用户角色进行授权
3. **用户绑定**: 存储桶和对象与用户 ID 关联
4. **种子数据**: Silo 启动时自动初始化 ObjectStorageAdmin 角色和权限

确保已正确配置 Users 模块的 JWT 设置（见上文配置章节）。

## 项目结构

```
modules/ObjectStorage/
├── src/
│   ├── Stargazer.Orleans.ObjectStorage.Domain/
│   │   └── Entities/
│   │       ├── Bucket.cs
│   │       ├── ObjectInfo.cs
│   │       ├── MultipartUpload.cs
│   │       └── BucketPolicy.cs
│   │
│   ├── Stargazer.Orleans.ObjectStorage.Grains.Abstractions/
│   │   ├── Authorization/
│   │   │   └── StoragePolicies.cs      # 权限策略常量
│   │   ├── IBucketGrain.cs
│   │   ├── IObjectGrain.cs
│   │   ├── Storage/
│   │   │   └── IStorageProvider.cs
│   │   └── Dtos/
│   │       ├── BucketDto.cs
│   │       ├── ObjectMetadataDto.cs
│   │       ├── UploadResultDto.cs
│   │       └── SignedUrlDto.cs
│   │
│   ├── Stargazer.Orleans.ObjectStorage.Grains/
│   │   ├── Grains/
│   │   │   ├── BucketGrain.cs
│   │   │   └── ObjectGrain.cs
│   │   └── SeedData/
│   │       └── StorageSeedDataInitializer.cs  # 种子数据初始化
│   │
│   ├── Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL/
│   │   ├── EfDbContext.cs
│   │   ├── Repository.cs
│   │   └── EntityFramworkCoreExtensions.cs
│   │
│   └── Stargazer.Orleans.ObjectStorage.Silo/
│       ├── Authorization/
│       │   └── StoragePermissionHandler.cs    # 权限处理器
│       ├── Controllers/
│       │   ├── BucketController.cs
│       │   └── ObjectController.cs
│       ├── Storage/
│       │   ├── LocalStorageProvider.cs
│       │   ├── AliyunOssProvider.cs
│       │   ├── AwsS3Provider.cs
│       │   ├── AzureBlobProvider.cs
│       │   ├── TencentCosProvider.cs
│       │   └── MinioProvider.cs
│       ├── Configuration/
│       │   └── StorageSettings.cs
│       ├── OrleansServerExtension.cs
│       └── Program.cs
│
├── tests/
│   └── Stargazer.Orleans.ObjectStorage.Tests/
│       ├── Configuration/
│       ├── Domain/
│       ├── Dto/
│       ├── Grains/
│       ├── Integration/                      # 集成测试
│       │   ├── TestWebApplicationFactory.cs
│       │   ├── IntegrationTestBase.cs
│       │   ├── BucketControllerIntegrationTests.cs
│       │   └── ObjectControllerIntegrationTests.cs
│       └── Storage/
│
└── README.md
```

## 权限实现

### StoragePolicies (权限策略常量)

```csharp
public static class StoragePolicies
{
    public static class Buckets
    {
        public const string View = "storage.bucket.view";
        public const string Create = "storage.bucket.create";
        public const string Update = "storage.bucket.update";
        public const string Delete = "storage.bucket.delete";
    }
    
    public static class Objects
    {
        public const string View = "storage.object.view";
        public const string Create = "storage.object.create";
        public const string Update = "storage.object.update";
        public const string Delete = "storage.object.delete";
    }
}
```

### StoragePermissionHandler (权限处理器)

实现 `IAuthorizationHandler`，验证用户是否拥有所需权限。

### StorageSeedDataInitializer (种子数据初始化)

实现 Orleans `IStartupTask`，在 Silo 启动时：
1. 创建存储桶/对象权限
2. 创建 ObjectStorageAdmin 角色并分配所有权限
3. 为 admin 用户分配 ObjectStorageAdmin 角色

## 测试

### 单元测试

| 测试类 | 说明 |
|--------|------|
| `StorageSettingsTests` | 配置解析、默认值、存储提供者创建 |
| `LocalStorageProviderTests` | 文件读写、目录管理、存在性检查 |
| `StorageProviderFactoryTests` | 工厂模式、未知提供者处理 |
| `EntityTests` | Bucket、ObjectInfo、MultipartUpload、BucketPolicy 实体 |
| `DtoTests` | DTO 属性映射和验证 |

运行单元测试：

```bash
dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests --filter "Category!=Integration"
```

### 集成测试

| 测试类 | 说明 |
|--------|------|
| `BucketControllerIntegrationTests` | 存储桶 CRUD、权限检查 |
| `ObjectControllerIntegrationTests` | 对象操作、签名 URL、分片上传 |

运行集成测试：

```bash
RUN_INTEGRATION_TESTS=true dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests --filter "Category=Integration"
```

> **测试总数**: 58 个测试

> **注意**: 运行集成测试需要：
> 1. 启动 PostgreSQL 和 Redis
> 2. 启动 Users Silo（端口 5079）
> 3. 启动 ObjectStorage Silo

## 注意事项

1. **删除存储桶**: 只有空存储桶才能被删除
2. **签名 URL**: 最长有效期为 7 天 (604800 秒)
3. **分片上传**: 分片信息有效期为 7 天
4. **并发控制**: 同一对象的并发上传/删除需要应用层处理
5. **MinIO Multipart Upload**: MinioProvider 的分片上传通过临时对象模拟实现，不使用原生 S3 分片上传 API
6. **JWT 配置**: 必须正确配置 JwtSettings，否则 API 请求会被拒绝
7. **初始化顺序**: Users 模块的种子数据需先执行，确保 admin 用户存在
