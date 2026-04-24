# Users 模块

基于 Orleans 框架的用户管理模块，提供用户注册、登录、角色管理、权限控制等功能。

## 功能特性

### 用户管理
- 用户注册与登录
- JWT Token 认证 (支持 AccessToken 和 RefreshToken)
- 密码验证与修改
- 用户资料查询
- 用户角色分配
- 用户状态管理（启用/禁用）

### 角色管理
- 创建、查询、更新、删除角色
- 角色权限分配
- 默认角色设置
- 角色优先级管理

### 权限控制
- 基于策略的权限检查
- 细粒度权限代码控制
- 权限类型分类 (Operation/Menu/Button/Api)
- 权限与角色关联

### 种子数据初始化
- Silo 启动时自动初始化权限
- 创建 Admin 角色并分配所有权限
- 创建默认 admin 用户 (账号: admin, 密码: Admin@123456)

## 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  ┌──────────────────┐  ┌──────────────────────────────┐     │
│  │ AccountController│  │    CurrentUserController    │     │
│  │  /api/account  │  │    /api/current-user        │     │
│  └────────┬───────┘  └─────────────┬────────────────┘     │
│           │                         │                        │
│  ┌───────┴───────┐  ┌──────────┴──────────────┐            │
│  │ UserController│  │    RoleController       │            │
│  │  /api/user  │  │    /api/role          │            │
│  └──────┬──────┘  └──────────┬───────────┘            │
│         │                    │                        │
│         │    ┌─────────────┴───────────┐                │
│         └───► │ PermissionController │                │
│              │  /api/permission   │                │
│              └─────────────────────────┘                │
└─────────────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                      Grain Layer                            │
│  ┌──────────────────┐  ┌────────────────────────────┐       │
│  │    UserGrain    │  │       RoleGrain        │       │
│  │  - Register   │  │  - CRUD Role           │       │
│  │  - Login     │  │  - Assign Permissions │       │
│  │  - Password  │  │  - Check Permission  │       │
│  │  - Profile   │  └──────────┬───────────┘       │
│  │  - Roles    │            │                  │
│  └───────────┼──────────┴──────────────────┐      │
│             │         ┌─────────────────────┐      │
│             └────────►│  PermissionGrain   │      │
│                      │  - CRUD Permission    │      │
│                      └─────────────────────┘      │
└──────────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   Repository Layer                          │
│  ┌──────────────────────────────────────────────┐       │
│  │           IRepository<TKey>                 │       │
│  │  - UserData / RoleData / PermissionData   │       │
│  │  - UserRoleData / RolePermissionData   │       │
│  └──────────────────────────────────────────┘       │
└─────────────────────────────────────────────────────┘
```

## 快速开始

### 0. JSON 约定

所有 API 请求和响应使用 **snake_case** 命名策略：

```json
{
  "account": "admin",
  "password": "Admin@123456",
  "is_active": true,
  "access_token": "eyJ..."
}
```

### 1. 数据库配置

在 `appsettings.json` 中配置连接字符串：

```json
{
  "ConnectionStrings": {
    "Users": "server=127.0.0.1;port=5432;Database=users;uid=postgres;pwd=123456",
    "Redis": "127.0.0.1:6379"
  }
}
```

确保 PostgreSQL 中已创建 `users` 数据库：

```sql
CREATE DATABASE users;
```

### 2. JWT 配置

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "Stargazer.Orleans.Users",
    "Audience": "Stargazer.Orleans.Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### 3. Silo 启动自动初始化

Silo 启动时自动初始化以下数据：

#### 权限 (14 个)
| 权限代码 | 名称 | 分类 | 描述 |
|---------|------|------|------|
| `users.view` | 查看用户 | 用户管理 | 查看用户列表 |
| `users.create` | 创建用户 | 用户管理 | 创建新用户 |
| `users.update` | 编辑用户 | 用户管理 | 编辑用户信息 |
| `users.delete` | 删除用户 | 用户管理 | 删除用户 |
| `users.assign_role` | 分配角色 | 用户管理 | 为用户分配角色 |
| `roles.view` | 查看角色 | 角色管理 | 查看角色列表 |
| `roles.create` | 创建角色 | 角色管理 | 创建新角色 |
| `roles.update` | 编辑角色 | 角色管理 | 编辑角色信息 |
| `roles.delete` | 删除角色 | 角色管理 | 删除角色 |
| `roles.assign_permission` | 分配权限 | 角色管理 | 为角色分配权限 |
| `permissions.view` | 查看权限 | 权限管理 | 查看权限列表 |
| `permissions.create` | 创建权限 | 权限管理 | 创建新权限 |
| `permissions.update` | 编辑权限 | 权限管理 | 编辑权限信息 |
| `permissions.delete` | 删除权限 | 权限管理 | 删除权限 |

#### 角色 (1 个)
- **Admin**: 系统管理员角色，拥有所有权限

#### 用户 (1 个)
- **admin**: 默认管理员账户
  - 账号: `admin`
  - 密码: `Admin@123456`
  - 拥有 Admin 角色

## API 接口

### 认证

账户接口无需认证，其他接口需要 JWT Token：

```
Authorization: Bearer <token>
```

### 账户接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| POST | `/api/account/register` | 用户注册 | 无 |
| POST | `/api/account/login` | 用户登录 | 无 |
| POST | `/api/account/refresh` | 刷新 Token | 无 |

#### 当前用户接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/current-user` | 获取当前用户 | `[Authorize]` |
| PUT | `/api/current-user` | 更新当前用户资料 | `[Authorize]` |
| POST | `/api/current-user/change-password` | 修改密码 | `[Authorize]` |
| GET | `/api/current-user/roles` | 获取当前用户角色 | `[Authorize]` |
| GET | `/api/current-user/permissions` | 获取当前用户权限 | `[Authorize]` |
| GET | `/api/current-user/has-permission/{permission}` | 检查权限 | `[Authorize]` |

### 用户接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/user/{id}` | 获取用户 | `users.view` |
| GET | `/api/user` | 分页查询用户 | `users.view` |
| POST | `/api/user` | 创建用户 | `users.create` |
| PUT | `/api/user/{id}` | 更新用户 | `users.update` |
| DELETE | `/api/user/{id}` | 删除用户 | `users.delete` |
| POST | `/api/user/{id}/roles` | 分配用户角色 | `users.assign_role` |
| GET | `/api/user/{id}/roles` | 获取用户角色 | `users.view` |
| GET | `/api/user/{id}/permissions` | 获取用户权限 | `users.view` |
| PATCH | `/api/user/{id}/status` | 更新用户状态 | `users.update` |

### 角色接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/role` | 分页查询角色 | `roles.view` |
| GET | `/api/role/{id}` | 获取角色 | `roles.view` |
| GET | `/api/role/active` | 获取所有激活角色 | `roles.view` |
| GET | `/api/role/{id}/permissions` | 获取角色权限 | `roles.view` |
| POST | `/api/role` | 创建角色 | `roles.create` |
| PUT | `/api/role/{id}` | 更新角色 | `roles.update` |
| DELETE | `/api/role/{id}` | 删除角色 | `roles.delete` |
| POST | `/api/role/{id}/permissions` | 分配角色权限 | `roles.assign_permission` |

### 权限接口

| 方法 | 路径 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/permission` | 分页查询权限 | `permissions.view` |
| GET | `/api/permission/{id}` | 获取权限 | `permissions.view` |
| GET | `/api/permission/category/{category}` | 按分类获取权限 | `permissions.view` |
| POST | `/api/permission` | 创建权限 | `permissions.create` |
| PUT | `/api/permission/{id}` | 更新权限 | `permissions.update` |
| DELETE | `/api/permission/{id}` | 删除权限 | `permissions.delete` |

### 请求示例

#### 用户注册
```bash
curl -X POST http://localhost:5000/api/account/register \
  -H "Content-Type: application/json" \
  -d '{
    "account": "newuser",
    "password": "Test@123456"
  }'
```

#### 用户登录
```bash
curl -X POST http://localhost:5000/api/account/login \
  -H "Content-Type: application/json" \
  -d '{
    "account": "admin",
    "password": "Admin@123456"
  }'
```

#### 刷新 Token
```bash
curl -X POST http://localhost:5000/api/account/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refresh_token": "your-refresh-token"
  }'
```

#### 获取当前用户信息
```bash
curl -X GET http://localhost:5000/api/current-user \
  -H "Authorization: Bearer <token>"
```

#### 修改密码
```bash
curl -X POST http://localhost:5000/api/current-user/change-password \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "old_password": "OldPassword@123",
    "new_password": "NewPassword@123"
  }'
```

#### 创建角色
```bash
curl -X POST http://localhost:5000/api/role \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CustomRole",
    "description": "Custom role",
    "priority": 100,
    "is_active": true
  }'
```

#### 更新用户状态 (启用/禁用)
```bash
# 禁用用户
curl -X PATCH http://localhost:5000/api/user/{userId}/status \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"is_enabled": false}'

# 启用用户
curl -X PATCH http://localhost:5000/api/user/{userId}/status \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"is_enabled": true}'
```

#### 分配角色权限
```bash
curl -X POST http://localhost:5000/api/role/{roleId}/permissions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '[guid1, guid2, guid3]'
```

#### 用户登录
```bash
curl -X POST http://localhost:5000/api/account/login \
  -H "Content-Type: application/json" \
  -d '{
    "name": "admin",
    "password": "Admin@123456"
  }'
```

#### 刷新 Token
```bash
curl -X POST http://localhost:5000/api/account/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token"
  }'
```

#### 获取当前用户信息
```bash
curl -X GET http://localhost:5000/api/current-user \
  -H "Authorization: Bearer <token>"
```

#### 修改密码
```bash
curl -X POST http://localhost:5000/api/current-user/change-password \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "oldPassword": "OldPassword@123",
    "newPassword": "NewPassword@123"
  }'
```

#### 创建角色
```bash
curl -X POST http://localhost:5000/api/role \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CustomRole",
    "description": "Custom role",
    "priority": 100,
    "isActive": true
  }'
```

#### 更新用户状态 (启用/禁用)
```bash
# 禁用用户
curl -X PATCH http://localhost:5000/api/user/{userId}/status \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"isEnabled": false}'

# 启用用户
curl -X PATCH http://localhost:5000/api/user/{userId}/status \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"isEnabled": true}'
```

#### 分配角色权限
```bash
curl -X POST http://localhost:5000/api/role/{roleId}/permissions \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '[guid1, guid2, guid3]'
```

### 响应格式

成功响应 (snake_case):
```json
{
  "code": "success",
  "message": "success",
  "data": { ... }
}
```

失败响应:
```json
{
  "code": "error_code",
  "message": "错误描述"
}
```

## 数据模型

### UserData (用户)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| Account | string | 账户名 (唯一) |
| Password | string | 密码哈希 (Argon2) |
| SecretKey | string | 密钥 |
| Name | string | 姓名 |
| Email | string | 邮箱 |
| PhoneNumber | string | 手机号 |
| Avatar | string | 头像 URL |
| CreatorId | GUID? | 创建者 ID |
| LastModifierId | GUID? | 最后修改者 ID |
| IsActive | bool | 是否激活 |
| IsDeleted | bool | 是否删除 |
| CreationTime | DateTime | 创建时间 |
| LastModifyTime | DateTime | 最后修改时间 |
| LastLoginTime | DateTime | 最后登录时间 |
| Roles | List<RoleData> | 关联角色 |

### RoleData (角色)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| Name | string | 角色名 (唯一) |
| Description | string | 描述 |
| IsDefault | bool | 是否默认角色 |
| Priority | int | 优先级 |
| IsActive | bool | 是否激活 |
| CreationTime | DateTime | 创建时间 |
| LastModifyTime | DateTime | 最后修改时间 |
| Permissions | List<PermissionData> | 关联权限 |
| UserRoles | List<UserRoleData> | 关联用户 |

### PermissionData (权限)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| Name | string | 权限名称 |
| Code | string | 权限代码 (唯一) |
| Description | string | 描述 |
| Category | string | 分类 |
| Type | PermissionType | 类型 (Operation/Menu/Button/Api) |
| IsActive | bool | 是否激活 |
| CreationTime | DateTime | 创建时间 |
| Roles | List<RoleData> | 关联角色 |

### UserRoleData (用户角色关联)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| UserId | GUID | 用户 ID |
| RoleId | GUID | 角色 ID |
| IsActive | bool | 是否激活 |
| CreationTime | DateTime | 创建时间 |

### RolePermissionData (角色权限关联)
| 字段 | 类型 | 描述 |
|------|------|------|
| Id | GUID | 主键 |
| RoleId | GUID | 角色 ID |
| PermissionId | GUID | 权限 ID |
| CreationTime | DateTime | 创建时间 |

## JWT Token 结构

### Access Token Claims
```json
{
  "sub": "user-guid",
  "jti": "token-guid",
  "name": "account",
  "userId": "user-guid",
  "account": "account",
  "role": "Admin",
  "iss": "Stargazer.Orleans.Users",
  "aud": "Stargazer.Orleans.Users",
  "exp": 1234567890
}
```

### Refresh Token
RefreshToken 使用 JWT 格式，包含：
- `jti`: JWT ID
- `iat`: 签发时间
- `exp`: 过期时间（默认 7 天）

### Token 响应格式
```json
{
  "success": true,
  "data": {
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expires_at": "2024-01-01T13:00:00Z",
    "user": {
      "id": "user-guid",
      "account": "username",
      "name": "User Name"
    }
  }
}
```

## 权限检查

### 策略配置
在 `PermissionHandler` 中定义权限策略：

```csharp
options.AddPolicy("permission:users.view", policy =>
    policy.Requirements.Add(new PermissionRequirement("users.view")));
```

### Grain 中使用
```csharp
public class SomeGrain : Grain, ISomeGrain
{
    private readonly IPermissionGrain _permissionGrain;

    public SomeGrain(IPermissionGrain permissionGrain)
    {
        _permissionGrain = permissionGrain;
    }

    public async Task<bool> CheckPermission(Guid userId, string permissionCode)
    {
        return await _permissionGrain.HasPermissionAsync(userId, permissionCode);
    }
}
```

### 权限策略常量

```csharp
public static class AuthorizationPermissions
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string Assign = "users.assign_role";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string Assign = "roles.assign_permission";
    }

    public static class Permissions
    {
        public const string View = "permissions.view";
        public const string Create = "permissions.create";
        public const string Update = "permissions.update";
        public const string Delete = "permissions.delete";
    }
}
```

## 项目结构

```
modules/Users/
├── src/
│   ├── Stargazer.Orleans.Users.Domain/
│   │   ├── Entity.cs
│   │   ├── IEntity.cs
│   │   ├── Users/
│   │   │   └── UserData.cs
│   │   ├── Roles/
│   │   │   ├── RoleData.cs
│   │   │   └── RolePermissionData.cs
│   │   ├── Permissions/
│   │   │   └── PermissionData.cs
│   │   └── UserRoles/
│   │       └── UserRoleData.cs
│   │
│   ├── Stargazer.Orleans.Users.Grains.Abstractions/
│   │   ├── PageResult.cs
│   │   ├── ResponseData.cs
│   │   ├── Authorization/
│   │   │   └── AuthorizationPermissions.cs
│   │   ├── Users/
│   │   │   ├── IUserGrain.cs
│   │   │   └── Dtos/
│   │   │       ├── UserDataDto.cs
│   │   │       ├── UserProfileDto.cs
│   │   │       ├── TokenResponseDto.cs
│   │   │       ├── RegisterAccountInputDto.cs
│   │   │       ├── RefreshTokenInputDto.cs
│   │   │       ├── ChangePasswordInputDto.cs
│   │   │       ├── VerifyPasswordInputDto.cs
│   │   │       ├── CreateOrUpdateUserInputDto.cs
│   │   │       ├── AssignRolesInputDto.cs
│   │   │       └── UpdateUserStatusInputDto.cs
│   │   │       └── UpdateProfileInputDto.cs
│   │   └── Roles/
│   │       ├── IRoleGrain.cs
│   │       ├── IPermissionGrain.cs
│   │       └── Dtos/
│   │           ├── RoleDataDto.cs
│   │           ├── PermissionDataDto.cs
│   │           └── CreateOrUpdateRoleInputDto.cs
│   │
│   ├── Stargazer.Orleans.Users.Grains/
│   │   ├── MapperProfile.cs
│   │   ├── Grains/
│   │   │   └── UserGrain.cs
│   │   ├── Roles/
│   │   │   ├── RoleGrain.cs
│   │   │   └── PermissionGrain.cs
│   │   └── SeedData/
│   │       └── UsersSeedDataInitializer.cs
│   │
│   ├── Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL/
│   │   ├── IRepository.cs
│   │   ├── Repository.cs
│   │   ├── EntityNotFoundException.cs
│   │   ├── DbContextModelCreatingExtensions.cs
│   │   ├── EfDbContext.cs
│   │   └── EntityFramworkCoreExtensions.cs
│   │
│   ├── Stargazer.Orleans.Users.EntityFrameworkCore.PostgreSQL.DbMigrations/
│   │   ├── EfDbMigrationsContext.cs
│   │   ├── DbContextFactory.cs
│   │   └── Migrations/
│   │
│   └── Stargazer.Orleans.Users.Silo/
│       ├── Controllers/
│       │   ├── AccountController.cs
│       │   ├── CurrentUserController.cs
│       │   ├── UserController.cs
│       │   ├── RoleController.cs
│       │   └── PermissionController.cs
│       ├── Authorization/
│       │   └── PermissionHandler.cs
│       ├── Security/
│       │   ├── JwtTokenService.cs
│       │   └── JwtSettings.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── OrleansServerExtension.cs
│       ├── OrleansClientExtension.cs
│       ├── OrleansOptions.cs
│       └── Program.cs
│
├── tests/
│   └── Stargazer.Orleans.Users.Tests/
│       ├── Integration/
│       │   ├── TestWebApplicationFactory.cs
│       │   ├── IntegrationTestBase.cs
│       │   ├── AccountControllerIntegrationTests.cs
│       │   ├── UserControllerIntegrationTests.cs
│       │   ├── RoleControllerIntegrationTests.cs
│       │   └── PermissionControllerIntegrationTests.cs
│       ├── Security/
│       │   └── JwtTokenServiceTests.cs
│       ├── Domain/
│       │   └── EntityTests.cs
│       └── Dto/
│           └── DtoTests.cs
│
└── README.md
```

## 测试

### 单元测试

Users 模块包含完整的单元测试：

| 测试类 | 说明 |
|--------|------|
| `JwtTokenServiceTests` | JWT Token 服务测试 |
| `EntityTests` | 实体测试（UserData、RoleData、PermissionData） |
| `DtoTests` | DTO 测试 |

运行单元测试：

```bash
dotnet test modules/Users/tests/Stargazer.Orleans.Users.Tests --filter "Category!=Integration"
```

### 集成测试

集成测试覆盖所有 Controller API：

| 测试类 | 说明 |
|--------|------|
| `AccountControllerIntegrationTests` | 账户接口测试 |
| `UserControllerIntegrationTests` | 用户接口测试 |
| `RoleControllerIntegrationTests` | 角色接口测试 |
| `PermissionControllerIntegrationTests` | 权限接口测试 |

运行集成测试：

```bash
RUN_INTEGRATION_TESTS=true dotnet test modules/Users/tests/Stargazer.Orleans.Users.Tests --filter "Category=Integration"
```

> **测试总数**: 94 个测试

> **注意**: 运行集成测试需要：
> 1. 启动 PostgreSQL 和 Redis
> 2. 启动 Orleans Silo
> 3. 或使用 Aspire Test Host

## 注意事项

1. **密码强度**: 密码必须至少 8 个字符，包含大小写字母和数字
2. **账户唯一性**: 账户名不能重复
3. **默认角色**: 新用户会自动分配默认角色
4. **软删除**: 用户和角色使用软删除，已删除数据不能重复使用
5. **Redis 集群**: 模块依赖 Redis 进行 Orleans 集群管理
6. **RefreshToken 格式**: RefreshToken 使用 JWT 格式，可通过 `ValidateToken` 验证
7. **初始化顺序**: Users 模块需先启动，确保 `Admin` 用户存在，再初始化其他业务模块
8. **Silo 注册**: 其他模块使用 Users 模块的权限系统时，需先确保 Users Silo 运行
9. **密码加密**: 密码使用 Argon2 算法加密存储