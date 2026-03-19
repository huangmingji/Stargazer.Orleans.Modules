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
- 对象列表查询 (支持前缀过滤)
- 对象存在性检查

### 高级功能
- **签名 URL**: 生成临时访问链接，支持自定义过期时间 (最长 7 天)
- **分片上传**: 大文件分片上传，支持断点续传
- **权限继承**: 基于存储桶 ACL 和用户角色自动授权

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
│  ┌──────────────────┐    ┌─────────────────────────────┐    │
│  │   BucketGrain    │    │      ObjectGrain            │    │
│  │  - CRUD Bucket   │    │  - Upload/Download          │    │
│  │  - ACL Check     │    │  - Multipart Upload         │    │
│  │  - Policy Eval   │    │  - Signed URL               │    │
│  └────────┬─────────┘    └─────────────┬───────────────┘    │
└───────────┼─────────────────────────────┼───────────────────┘
            │                             │
            ▼                             ▼
┌─────────────────────────────────────────────────────────────┐
│                   Repository Layer                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              IRepository<TKey>                       │   │
│  │  - Bucket / BucketPolicy                             │   │
│  │  - ObjectInfo / MultipartUpload                      │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Storage Provider Layer                     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│  │  Local   │ │ Aliyun   │ │   AWS    │ │  Azure   │ ...    │
│  │   OSS    │ │   S3     │ │  Blob    │ │   COS    │        │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘        │
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

### 3. 配置权限策略

存储模块使用 Users 模块的权限系统，需在数据库中配置以下权限：

| 权限名称 | 描述 |
|---------|------|
| `storage.bucket.view` | 查看存储桶 |
| `storage.bucket.create` | 创建存储桶 |
| `storage.bucket.update` | 更新存储桶 |
| `storage.bucket.delete` | 删除存储桶 |
| `storage.object.view` | 查看/下载对象 |
| `storage.object.create` | 上传对象 |
| `storage.object.update` | 更新对象 |
| `storage.object.delete` | 删除对象 |

## API 接口

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
| GET | `/api/storage/object/{bucketId}/{key}/metadata` | 获取对象元数据 | storage.object.view |
| GET | `/api/storage/object/{bucketId}` | 列出对象 | storage.object.view |
| POST | `/api/storage/object/{bucketId}/{key}` | 上传对象 | storage.object.create |
| PUT | `/api/storage/object/{bucketId}/{key}` | 更新对象 | storage.object.update |
| DELETE | `/api/storage/object/{bucketId}/{key}` | 删除对象 | storage.object.delete |
| GET | `/api/storage/object/{bucketId}/{key}/signed-url` | 获取签名URL | storage.object.view |
| POST | `/api/storage/object/{bucketId}/{key}/multipart/initiate` | 初始化分片上传 | storage.object.create |
| POST | `/api/storage/object/{bucketId}/{key}/multipart/{uploadId}/part` | 上传分片 | storage.object.create |
| POST | `/api/storage/object/{bucketId}/{key}/multipart/{uploadId}/complete` | 完成分片上传 | storage.object.create |
| DELETE | `/api/storage/object/{bucketId}/{key}/multipart/{uploadId}` | 取消分片上传 | storage.object.delete |

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
curl -X GET "http://localhost:5000/api/storage/object/{bucketId}/my-file.txt/signed-url?expiry=01:00:00&method=GET" \
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

**注意**: 目前只有 `LocalStorageProvider` 可完全正常使用。其他云存储提供者 (Aliyun/AWS/Azure/Tencent/MinIO) 等待对应 SDK 支持 .NET 10 后可用。

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

确保已正确配置 Users 模块的 JWT 设置：

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "Stargazer.Orleans",
    "Audience": "Stargazer.Orleans",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryMinutes": 10080
  }
}
```

## 项目结构

```
modules/ObjectStorage/
├── src/
│   ├── Stargazer.Orleans.ObjectStorage.Domain/
│   │   ├── Entities/
│   │   │   ├── Bucket.cs
│   │   │   ├── ObjectInfo.cs
│   │   │   ├── MultipartUpload.cs
│   │   │   └── BucketPolicy.cs
│   │   ├── Entity.cs
│   │   └── IEntity.cs
│   │
│   ├── Stargazer.Orleans.ObjectStorage.Grains.Abstractions/
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
│   │   └── Grains/
│   │       ├── BucketGrain.cs
│   │       └── ObjectGrain.cs
│   │
│   ├── Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL/
│   │   ├── EfDbContext.cs
│   │   ├── Repository.cs
│   │   └── EntityFramworkCoreExtensions.cs
│   │
│   └── Stargazer.Orleans.ObjectStorage.Silo/
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
└── README.md
```

## 单元测试

ObjectStorage 模块包含完整的单元测试，涵盖配置、存储提供者、数据模型和 DTO。

### 测试结构

```
modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests/
├── Configuration/
│   └── StorageSettingsTests.cs (8 tests)
├── Storage/
│   ├── LocalStorageProviderTests.cs (13 tests)
│   └── StorageProviderFactoryTests.cs (3 tests)
├── Domain/
│   └── EntityTests.cs (8 tests)
└── Dto/
    └── DtoTests.cs (7 tests)
```

### 运行测试

```bash
# 运行所有测试
dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests

# 运行特定测试类
dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests --filter "FullyQualifiedName~LocalStorageProviderTests"

# 运行单个测试
dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests --filter "FullyQualifiedName~LocalStorageProviderTests.FileExists_ReturnsFalseForNonExistentFile"

# 查看详细输出
dotnet test modules/ObjectStorage/tests/Stargazer.Orleans.ObjectStorage.Tests --verbosity normal
```

### 测试覆盖

| 测试类 | 测试内容 |
|--------|----------|
| **StorageSettingsTests** | 配置解析、默认值、存储提供者创建 |
| **LocalStorageProviderTests** | 文件读写、目录管理、存在性检查 |
| **StorageProviderFactoryTests** | 工厂模式、未知提供者处理 |
| **EntityTests** | Bucket、ObjectInfo、MultipartUpload、BucketPolicy 实体 |
| **DtoTests** | DTO 属性映射和验证 |

### 测试结果

```
Passed! - Failed: 0, Passed: 39, Skipped: 0, Total: 39
```

### 测试详情

#### StorageSettingsTests (8 tests)
- `DefaultExpiryMinutes_ReturnsCorrectValue` - 验证默认过期时间
- `DefaultExpiryMinutes_Returns60WhenNotConfigured` - 验证未配置时的默认值
- `MaxSignedUrlExpirySeconds_Returns7Days` - 验证最大签名 URL 过期时间
- `MaxSignedUrlExpirySeconds_Returns604800` - 验证最大过期秒数
- `MultipartUploadExpiryMinutes_Returns7Days` - 验证分片上传过期时间
- `MultipartUploadExpiryMinutes_Returns10080WhenNotConfigured` - 验证默认值
- `CreateStorageProvider_ThrowsForUnknownProvider` - 验证未知提供者抛出异常
- `CreateStorageProvider_CreatesLocalProvider` - 验证本地提供者创建

#### LocalStorageProviderTests (13 tests)
- `FileExists_ReturnsTrueForExistingFile` - 验证已存在文件检测
- `FileExists_ReturnsFalseForNonExistentFile` - 验证不存在文件检测
- `UploadAsync_SavesFileToCorrectPath` - 验证文件上传路径
- `UploadAsync_CreatesParentDirectories` - 验证父目录自动创建
- `UploadAsync_StoresMetadata` - 验证元数据存储
- `DownloadAsync_ReturnsFileContent` - 验证文件下载
- `DeleteAsync_RemovesFile` - 验证文件删除
- `GetSignedUrl_ReturnsUrl` - 验证签名 URL 生成
- `ListObjectsAsync_ReturnsObjectList` - 验证对象列表
- `DirectoryExists_ReturnsTrueForExistingDirectory` - 验证目录存在检查
- `CreateDirectory_CreatesDirectory` - 验证目录创建
- `DeleteDirectory_RemovesDirectory` - 验证目录删除
- `GetStorageStats_ReturnsCorrectStats` - 验证存储统计

#### StorageProviderFactoryTests (3 tests)
- `CreateProvider_ReturnsLocalStorageProvider` - 验证返回本地存储提供者
- `CreateProvider_ThrowsForUnknownProvider` - 验证未知提供者异常
- `GetTypeName_ReturnsCorrectType` - 验证类型名称获取

#### EntityTests (8 tests)
- `Bucket_DefaultValues_AreCorrect` - 验证 Bucket 默认值
- `Bucket_CanSetProperties` - 验证 Bucket 属性设置
- `ObjectInfo_DefaultValues_AreCorrect` - 验证 ObjectInfo 默认值
- `ObjectInfo_CanSetProperties` - 验证 ObjectInfo 属性设置
- `MultipartUpload_DefaultValues_AreCorrect` - 验证 MultipartUpload 默认值
- `MultipartUpload_CanSetProperties` - 验证 MultipartUpload 属性设置
- `BucketPolicy_DefaultValues_AreCorrect` - 验证 BucketPolicy 默认值
- `BucketPolicy_CanSetProperties` - 验证 BucketPolicy 属性设置

#### DtoTests (7 tests)
- `BucketDto_CanSetProperties` - 验证 BucketDto 属性设置
- `ObjectMetadataDto_CanSetProperties` - 验证 ObjectMetadataDto 属性设置
- `UploadResultDto_CanSetProperties` - 验证 UploadResultDto 属性设置
- `SignedUrlDto_CanSetProperties` - 验证 SignedUrlDto 属性设置
- `CreateBucketInputDto_CanSetProperties` - 验证 CreateBucketInputDto 属性设置
- `CreateObjectInputDto_CanSetProperties` - 验证 CreateObjectInputDto 属性设置
- `UpdateBucketInputDto_CanSetProperties` - 验证 UpdateBucketInputDto 属性设置

## 注意事项

1. **删除存储桶**: 只有空存储桶才能被删除
2. **签名 URL**: 最长有效期为 7 天 (604800 秒)
3. **分片上传**: 分片信息有效期为 7 天
4. **并发控制**: 同一对象的并发上传/删除需要应用层处理
5. **.NET 10 SDK**: 云存储 SDK 需要等待官方支持
