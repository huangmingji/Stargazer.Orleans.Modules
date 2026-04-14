# Stargazer.Orleans.Modules: 基于 Microsoft Orleans 的模块化微服务框架

> 这是一个生产级别的开源项目，展示了如何使用 Microsoft Orleans 构建可扩展的模块化微服务架构。

## 项目概述

Stargazer.Orleans.Modules 是一个基于 Microsoft Orleans 框架构建的分布式应用项目模板，集成了三个核心业务模块：

- **Users 模块** - 用户认证与权限管理
- **MessageManagement 模块** - 多渠道消息发送
- **ObjectStorage 模块** - 分布式对象存储

### 技术栈

| 技术 | 用途 |
|------|------|
| .NET 10 | 运行时 |
| Microsoft Orleans 10.x | 分布式框架 |
| PostgreSQL | 主数据库 |
| Entity Framework Core | ORM |
| JWT | 身份认证 |
| Redis | Orleans 集群/定时器 |
| xUnit | 单元测试 |

## 为什么选择 Microsoft Orleans？

传统的微服务架构面临诸多挑战：

1. **服务间通信复杂** - 需要处理服务发现、负载均衡、重试等
2. **状态管理困难** - 分布式状态的一致性问题
3. **开发效率低** - 大量样板代码

Orleans 提供了独特的 **Virtual Actor** 模型，解决了这些问题：

- **单线程执行** - Actor 内置线程安全，无需锁
- **自动状态持久化** - Actor 状态由 Orleans 自动管理
- **位置透明性** - 调用方无需知道 Actor 位置
- **位置透明激活** - Actor 可在任意 Silo 中运行

## 模块化架构设计

### 通用项目结构

每个业务模块都遵循六层架构：

```
┌─────────────────────────────────────────────────────────────┐
│                     API Layer (Silo)                         │
│  Controllers  - HTTP 请求处理，JWT 认证，权限检查              │
└───────────────────────────────┬─────────────────────────────┘
                                 │ IClusterClient
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│               Application Layer (Grains.Abstractions)        │
│  IGrain 接口  - 业务接口定义                                 │
│  DTOs        - 数据传输对象                                 │
└───────────────────────────────┬─────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│                   Business Layer (Grains)                    │
│  Grain 实现  - 核心业务逻辑                                 │
│  Sender      - 外部服务调用 (可选)                           │
└───────────────────────────────┬─────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                             │
│  Entity       - 核心业务实体                               │
│  Value Object - 值对象                                     │
└───────────────────────────────┬─────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  Repository   - 数据访问抽象                                │
│  EF Core     - PostgreSQL 持久化                           │
└─────────────────────────────────────────────────────────────┘
```

### 项目引用关系

```
Stargazer.Orleans.Modules.sln
├── aspire/                           # Aspire 服务编排
├── modules/
│   ├── Users/                        # 基础模块（其他模块依赖）
│   │   ├── Domain/
│   │   ├── Grains.Abstractions/
│   │   ├── Grains/
│   │   ├── EntityFrameworkCore/
│   │   ├── Silo/                    # API + JWT 认证
│   │   └── Tests/
│   │
│   ├── MessageManagement/            # 业务模块
│   │   ├── Domain/
│   │   ├── Grains.Abstractions/
│   │   ├── Grains/
│   │   ├── EntityFrameworkCore/
│   │   ├── Silo/                    # 依赖 Users 模块
│   │   └── Tests/
│   │
│   └── ObjectStorage/                # 业务模块
│       ├── Domain/
│       ├── Grains.Abstractions/
│       ├── Grains/
│       ├── EntityFrameworkCore/
│       ├── Silo/                    # 依赖 Users 模块
│       └── Tests/
```

## 模块详解

### 1. Users 模块 - 身份认证与权限管理

Users 模块是整个系统的基础，其他模块都依赖它进行身份认证和权限检查。

#### 核心功能

| 功能 | 描述 |
|------|------|
| 用户管理 | 注册、登录、CRUD、状态管理 |
| 角色管理 | 创建角色、分配权限、优先级 |
| 权限控制 | 基于策略的细粒度权限 |
| JWT 认证 | AccessToken + RefreshToken |

#### 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                    API Layer                               │
│  AccountController  ───  UserController                  │
│  RoleController      ───  PermissionController             │
└───────────────────────────────┬───────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│                     Grain Layer                            │
│  UserGrain          ───  RoleGrain                       │
│  PermissionGrain                                           │
└───────────────────────────────┬───────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────┐
│                   Repository Layer                         │
│  UserData / RoleData / PermissionData                     │
└─────────────────────────────────────────────────────────────┘
```

#### 权限模型

```
UserData (用户)
    │
    ▼ (N:M)
UserRoleData (用户角色关联)
    │
    ▼
RoleData (角色)
    │
    ▼ (N:M)
RolePermissionData (角色权限关联)
    │
    ▼
PermissionData (权限)
```

#### API 接口

```
/api/account/register     - 用户注册
/api/account/login       - 用户登录
/api/account/refresh     - 刷新 Token
/api/user               - 用户 CRUD
/api/role               - 角色 CRUD
/api/permission         - 权限 CRUD
```

#### 种子数据

Silo 启动时自动初始化：

- **13 个权限**: `users.view`, `users.create`, `roles.view`, ...
- **Admin 角色**: 拥有所有权限
- **admin 用户**: 默认管理员 (账号: `admin`, 密码: `Admin@123456`)

### 2. MessageManagement 模块 - 多渠道消息发送

#### 核心功能

| 功能 | 描述 |
|------|------|
| 多渠道发送 | Email、SMS、Push |
| 多 Provider | 阿里云、腾讯云、华为云、天翼云 |
| 模板管理 | `{{variable}}` 占位符 |
| 定时发送 | Redis Reminder 持久化 |
| 批量发送 | 一次请求发送给多个接收者 |

#### 支持的 Provider

| 通道 | Provider | SDK/认证方式 |
|------|----------|--------------|
| Email | SMTP | MailKit |
| SMS | 阿里云 | AlibabaCloud SDK |
| SMS | 腾讯云 | TencentCloud SDK 3.0 |
| SMS | 华为云 | HTTP API (WSSE) |
| SMS | 天翼云 | HTTP API (HMAC-SHA256) |
| Push | 极光推送 | HTTP API v3 |
| Push | 友盟推送 | HTTP API (MD5 Sign) |

#### 消息发送流程

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  API Request │ ──▶ │  MessageGrain │ ──▶ │  Template    │
└──────────────┘     └──────┬───────┘     └──────────────┘
                             │
              ┌─────────────┴─────────────┐
              │                           │
              ▼                           ▼
     ┌──────────────┐          ┌──────────────┐
     │  Scheduled?  │          │  Provider   │
     │  (Redis)     │          │  Factory    │
     └──────────────┘          └──────┬───────┘
                                      │
              ┌───────────────────────┼───────────────────────┐
              │                       │                       │
              ▼                       ▼                       ▼
        ┌──────────┐          ┌──────────┐          ┌──────────┐
        │  Aliyun  │          │ Tencent  │          │  Huawei  │
        │   SMS    │          │   SMS    │          │   SMS    │
        └──────────┘          └──────────┘          └──────────┘
```

### 3. ObjectStorage 模块 - 分布式对象存储

#### 核心功能

| 功能 | 描述 |
|------|------|
| 存储桶管理 | CRUD、ACL、策略 |
| 对象存储 | 上传、下载、删除 |
| 签名 URL | 临时访问链接 |
| 分片上传 | 大文件断点续传 |
| 多 Provider | Local、阿里云、AWS、Azure、腾讯云、MinIO |

#### 存储 Provider 架构

```
┌─────────────────────────────────────────────────────────────┐
│                  Storage Provider Layer                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │  Local   │ │ Aliyun   │ │   AWS    │ │  Azure   │   │
│  │   OSS    │ │   S3     │ │  Blob    │ │   COS    │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘   │
│                                                          │
│  ┌──────────┐ ┌──────────┐                              │
│  │ Tencent  │ │  MinIO   │                              │
│  │   COS    │ │   S3     │                              │
│  └──────────┘ └──────────┘                              │
└─────────────────────────────────────────────────────────────┘
```

#### 权限模型

ObjectStorage 模块使用 Users 模块的权限体系：

| 权限代码 | 描述 |
|----------|------|
| `storage.bucket.view` | 查看存储桶 |
| `storage.bucket.create` | 创建存储桶 |
| `storage.bucket.update` | 更新存储桶 |
| `storage.bucket.delete` | 删除存储桶 |
| `storage.object.view` | 查看对象 |
| `storage.object.create` | 上传对象 |
| `storage.object.update` | 更新对象 |
| `storage.object.delete` | 删除对象 |

## Orleans 特性应用

### 1. Grain 有状态设计

```csharp
public class UserGrain : Grain, IUserGrain
{
    private UserData? _state;
    
    public async Task<UserData?> GetUserAsync()
    {
        if (_state == null)
        {
            // 首次访问时自动从存储加载
            _state = await userRepository.FindAsync(this.GetPrimaryKey());
        }
        return _state;
    }
}
```

**关键点**：
- `_state` 字段由 Orleans 自动持久化
- 无需手动实现缓存失效逻辑
- 单线程执行保证线程安全

### 2. StatelessWorker 无状态 Grain

```csharp
[StatelessWorker]
public class MessageGrain : Grain, IMessageGrain
{
    // 无状态 Grain 可以在任意 Silo 中运行
    // 适合无状态的服务如消息发送
}
```

**适用场景**：
- 无需维护状态的业务逻辑
- 高并发、请求量大的接口
- 消息发送等一次性操作

### 3. Reminder 定时任务

```csharp
public class ScheduledMessageReminderGrain : Grain, IRemindable
{
    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        // 定时检查并发送待发送的消息
        var pendingMessages = await messageRepository.FindPendingAsync();
        foreach (var message in pendingMessages)
        {
            await SendMessageAsync(message);
        }
    }
}
```

**优势**：
- **持久化**：Reminder 信息存储在 Redis/数据库
- **Silo 重启后恢复**：不丢失定时任务
- **分布式协调**：多 Silo 环境正确触发

### 4. Grain 互调用

```csharp
public class ObjectGrain : Grain, IObjectGrain
{
    private readonly IClusterClient _clusterClient;
    
    public async Task<bool> CheckPermissionAsync(Guid userId, string permission)
    {
        // 调用 Users 模块的 PermissionGrain
        var permissionGrain = _clusterClient.GetGrain<IPermissionGrain>(0);
        return await permissionGrain.HasPermissionAsync(userId, permission);
    }
}
```

## 权限与安全

### JWT 认证流程

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Client     │ ──▶ │ Middleware   │ ──▶ │  Controller  │
│  (Request)  │     │ (Validate)   │     │  (Authorize) │
└──────────────┘     └──────────────┘     └──────────────┘
                            │
                            ▼
                     ┌──────────────┐
                     │  Claims      │
                     │  userId, roles│
                     └──────────────┘
```

### PermissionHandler 实现

```csharp
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = GetUserId(context.User);
        var permissionGrain = _clusterClient.GetGrain<IPermissionGrain>(0);
        
        var hasPermission = await permissionGrain.HasPermissionAsync(
            userId, requirement.PermissionCode);
            
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
```

### 控制器使用

```csharp
[ApiController]
[Authorize]
public class BucketController : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(policy: "permission:storage.bucket.view")]
    public async Task<IActionResult> GetBucket(Guid id)
    {
        // 权限检查自动完成
    }
}
```

## 种子数据初始化

### IStartupTask 接口

每个模块都实现 `IStartupTask` 接口，在 Silo 启动时自动初始化数据：

```csharp
public class StorageSeedDataInitializer(
    IClusterClient clusterClient,
    ILogger<StorageSeedDataInitializer> logger) : IStartupTask
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        // 1. 创建权限
        var permissionIds = await SeedPermissionsAsync();
        
        // 2. 创建角色
        var role = await SeedAdminRoleAsync(permissionIds);
        
        // 3. 分配给 admin 用户
        await AssignRoleToAdminAsync(role.Id);
    }
}
```

### 注册到 Silo

```csharp
// OrleansServerExtension.cs
builder.UseOrleans(siloBuilder =>
{
    // ... 其他配置
    siloBuilder.AddStartupTask<StorageSeedDataInitializer>();
});
```

## 测试策略

### 单元测试

```
├── StorageSettingsTests.cs        # 配置测试
├── LocalStorageProviderTests.cs  # Provider 测试
├── EntityTests.cs                # 实体测试
└── DtoTests.cs                  # DTO 测试
```

### 集成测试

```csharp
public class BucketControllerIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateBucket_WithValidData_ReturnsSuccess()
    {
        // 1. 登录获取 Token
        await LoginAsAdminAsync();
        
        // 2. 调用 API
        var (success, data, _) = await PostAsync<BucketDto>(
            "api/storage/bucket", bucket);
        
        // 3. 验证结果
        Assert.True(success);
        Assert.NotNull(data);
    }
}
```

### 测试统计

| 模块 | 单元测试 | 集成测试 | 总计 |
|------|----------|----------|------|
| Users | ~80 | ~50 | 94 |
| MessageManagement | ~100 | ~30 | 124 |
| ObjectStorage | ~40 | ~20 | 58 |

## 快速开始

### 1. 环境准备

```bash
# 安装 .NET 10 SDK
dotnet --version

# 启动基础设施 (docker-compose)
cd database-setup-script
docker-compose up -d
```

### 2. 运行服务

```bash
# 1. 启动 Users 模块 (必须先启动)
dotnet run --project modules/Users/src/Stargazer.Orleans.Users.Silo

# 2. 启动其他模块 (可并行)
dotnet run --project modules/MessageManagement/src/...
dotnet run --project modules/ObjectStorage/src/...
```

### 3. 运行测试

```bash
# 运行所有单元测试
dotnet test --filter "Category!=Integration"

# 运行集成测试
RUN_INTEGRATION_TESTS=true dotnet test --filter "Category=Integration"
```

## 项目亮点

### 1. 真正的模块化

- 各模块完全独立，可单独部署
- 通过接口依赖而非实现依赖
- 支持动态添加新模块

### 2. 权限体系复用

```
Users 模块 (权限基础设施)
         │
         ├──▶ MessageManagement 模块
         │
         └──▶ ObjectStorage 模块
```

业务模块只需定义权限代码，Users 模块自动处理权限检查。

### 3. 存储 Provider 可扩展

通过策略模式和工厂模式，轻松添加新的存储 Provider：

```csharp
public interface IStorageProvider
{
    Task<string> UploadAsync(Stream data, string key);
    Task<Stream?> DownloadAsync(string key);
    // ...
}

// 添加新 Provider 只需：
// 1. 实现 IStorageProvider
// 2. 在 StorageProviderFactory 中注册
```

### 4. 完整的测试覆盖

- 单元测试：业务逻辑独立测试
- 集成测试：端到端 API 测试
- 总计 **276 个测试**

## 总结

Stargazer.Orleans.Modules 展示了：

1. **Orleans 最佳实践** - Grain 设计、状态管理、定时任务
2. **模块化架构** - 低耦合、高内聚
3. **企业级功能** - JWT 认证、细粒度权限、多租户存储
4. **可扩展设计** - Provider 模式、策略模式

这个项目是构建生产级 Orleans 应用的优秀参考。

## 相关资源

- [Microsoft Orleans 官方文档](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [项目 GitHub](https://github.com/huangmingji/Stargazer.Orleans.Modules)
- [Orleans 社区](https://gitter.im/dotnet/orleans)
