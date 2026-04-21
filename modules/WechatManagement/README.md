# WechatManagement 模块

微信公众号管理模块，基于 Orleans 框架开发。

## 功能特性

### 公众号管理
- 公众号账号的 CRUD 操作
- AccessToken 管理（自动刷新）
- 微信服务器回调验证

### 用户管理
- 粉丝列表查询（支持分页、状态筛选）
- 用户关注/取消关注事件处理
- 用户标签管理
- 用户分组管理

### 消息管理
- 模板消息发送
- 客服消息发送
- 群发消息发送
- 被动回复

### 微信扫码登录
- 二维码生成
- 扫码回调处理
- 微信用户与本地账号绑定/解绑

## 模块结构

```
WechatManagement/
├── src/
│   ├── Stargazer.Orleans.WechatManagement.Domain/          # 领域实体
│   ├── Stargazer.Orleans.WechatManagement.Domain.Share/     # 共享领域模型
│   ├── Stargazer.Orleans.WechatManagement.Grains/            # Orleans Grains 实现
│   │   ├── Accounts/                                       # 公众号账号 Grains
│   │   ├── Users/                                           # 用户 Grains
│   │   └── Messages/                                         # 消息 Grains
│   ├── Stargazer.Orleans.WechatManagement.Grains.Abstractions/  # Grain 接口
│   ├── Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL/  # EF Core 仓储
│   └── Stargazer.Orleans.WechatManagement.Silo/              # API 控制器
```

## API 接口

### 公众号管理

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/accounts` | 获取公众号列表 |
| GET | `/api/wechat/accounts/{id}` | 获取公众号详情 |
| POST | `/api/wechat/accounts` | 创建公众号 |
| PUT | `/api/wechat/accounts/{id}` | 更新公众号 |
| DELETE | `/api/wechat/accounts/{id}` | 删除公众号 |

### 粉丝管理

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/{accountId}/fans` | 获取粉丝列表 |
| GET | `/api/wechat/{accountId}/fans/{id}` | 获取粉丝详情 |

### 标签管理

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/{accountId}/tags` | 获取标签列表 |
| POST | `/api/wechat/{accountId}/tags` | 创建标签 |
| PUT | `/api/wechat/{accountId}/tags/{id}` | 更新标签 |
| DELETE | `/api/wechat/{accountId}/tags/{id}` | 删除标签 |

### 分组管理

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/{accountId}/groups` | 获取分组列表 |
| POST | `/api/wechat/{accountId}/groups` | 创建分组 |
| PUT | `/api/wechat/{accountId}/groups/{id}` | 更新分组 |
| DELETE | `/api/wechat/{accountId}/groups/{id}` | 删除分组 |

### 消息发送

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `/api/wechat/{accountId}/messages/template` | 发送模板消息 |
| POST | `/api/wechat/{accountId}/messages/custom` | 发送客服消息 |
| POST | `/api/wechat/{accountId}/messages/mass` | 发送群发消息 |

### 微信回调

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/{accountId}/callback` | 验证服务器 |
| POST | `/api/wechat/{accountId}/callback` | 接收微信消息 |

### 微信扫码登录

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/wechat/{accountId}/auth/qrcode` | 获取登录二维码 |
| POST | `/api/wechat/{accountId}/auth/bind` | 绑定本地账号 |
| POST | `/api/wechat/{accountId}/auth/unbind` | 解绑微信用户 |
| GET | `/api/wechat/{accountId}/auth/status` | 获取绑定状态 |
| POST | `/api/wechat/{accountId}/auth/callback` | 扫码回调处理 |

## 微信事件处理

### 用户关注事件
当用户关注公众号时，系统会自动：
1. 接收微信推送的关注事件
2. 调用微信 API 获取用户详细信息
3. 保存用户到数据库

### 用户取消关注事件
当用户取消关注公众号时，系统会自动更新用户状态。

## 微信扫码登录流程

1. **获取二维码**: 用户访问 `/api/wechat/{accountId}/auth/qrcode` 获取登录二维码
2. **扫码授权**: 用户在微信中扫描二维码并授权
3. **回调处理**: 微信服务器推送扫码结果到 `/api/wechat/{accountId}/auth/callback`
4. **绑定账号**: 前端调用 `/api/wechat/{accountId}/auth/bind` 绑定本地账号
5. **获取 Token**: 绑定成功后返回本地系统的认证 Token

### 微信用户绑定

系统使用 `WechatUserBinding` 实体存储微信用户与本地用户的绑定关系：
- `WechatUserId` - 微信用户 ID
- `LocalUserId` - 本地用户 ID
- `AccountId` - 公众号 ID
- `OpenId` - 微信 OpenId
- `BindingTime` - 绑定时间
- `IsActive` - 绑定状态

## 配置

### appsettings.json

```json
{
  "Orleans": {
    "ClusterId": "wechat-cluster",
    "ServiceId": "wechat-service",
    "SiloPort": 11111,
    "GatewayPort": 30000
  },
  "ConnectionStrings": {
    "Wechat": "Host=localhost;Database=wechat;Username=postgres;Password=password"
  }
}
```

## 授权策略

模块使用基于策略的授权：

| 策略名称 | 说明 |
|----------|------|
| `permission:accounts.view` | 查看公众号 |
| `permission:accounts.create` | 创建公众号 |
| `permission:accounts.update` | 更新公众号 |
| `permission:accounts.delete` | 删除公众号 |
| `permission:fans.view` | 查看粉丝 |
| `permission:fans.update` | 更新粉丝 |
| `permission:fans.tag` | 管理粉丝标签 |
| `permission:groups.view` | 查看分组 |
| `permission:groups.create` | 创建分组 |
| `permission:groups.update` | 更新分组 |
| `permission:groups.delete` | 删除分组 |
| `permission:tags.view` | 查看标签 |
| `permission:tags.create` | 创建标签 |
| `permission:tags.update` | 更新标签 |
| `permission:tags.delete` | 删除标签 |
| `permission:messages.send_template` | 发送模板消息 |
| `permission:messages.send_custom` | 发送客服消息 |
| `permission:messages.send_mass` | 发送群发消息 |

## 技术栈

- **.NET 10**
- **Orleans** - 分布式框架
- **Entity Framework Core** - PostgreSQL
- **Senparc.Weixin** - 微信 SDK
- **ASP.NET Core** - Web 框架