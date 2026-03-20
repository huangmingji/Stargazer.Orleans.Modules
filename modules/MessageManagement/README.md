# Stargazer.Orleans.MessageManagement

基于 Orleans 的消息管理模块，支持邮件、短信、推送通知的统一发送与管理。

## 功能特性

- **多渠道消息发送**：支持 Email、SMS、Push 三种消息通道
- **多 Provider 支持**：每个通道可配置多个 Provider（阿里云、腾讯云、华为云、天翼云等）
- **模板管理**：支持消息模板创建、编辑、预览，支持 `{{variable}}` 占位符
- **批量发送**：支持批量发送消息到多个接收者
- **定时发送**：支持设置消息的定时发送时间
- **重试机制**：支持对失败消息进行重试
- **发送记录**：完整记录消息发送历史，支持查询和筛选
- **Provider 路由**：支持按 Provider 名称路由，也支持默认 Provider

## 项目结构

```
Stargazer.Orleans.MessageManagement/
├── src/
│   ├── Stargazer.Orleans.MessageManagement.Domain/          # 领域实体
│   ├── Stargazer.Orleans.MessageManagement.Grains/         # Grain 实现 + Senders
│   ├── Stargazer.Orleans.MessageManagement.Grains.Abstractions/  # 接口和 DTO
│   ├── Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL/  # EF Core 持久化
│   └── Stargazer.Orleans.MessageManagement.Silo/            # API 控制器和配置
└── tests/
```

## 架构设计

### 分层架构

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API Layer (Silo)                               │
│  MessageController  ───  TemplateController                                 │
│  接收 HTTP 请求，调用 Orleans Grain                                           │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │ IClusterClient.GetGrain<IMessageGrain>()
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Application Layer (Grains.Abstractions)                 │
│  IMessageGrain  ───  ITemplateGrain                                         │
│  定义业务接口和数据传输对象                                                     │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Business Layer (Grains)                            │
│  MessageGrain  ───  TemplateGrain  ───  Sender Factories                    │
│  核心业务逻辑：消息发送、模板管理、Provider 路由                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  Senders/                                                           │    │
│  │  ├── Email/    (SmtpEmailSender)                                    │    │
│  │  ├── Sms/     (Aliyun/Tencent/Huawei/Ctyun Senders)                 │    │
│  │  └── Push/    (JPush/Umeng - 预留)                                   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Domain Layer                                       │
│  MessageRecord  ───  MessageTemplate  ───  ProviderConfig                   │
│  核心业务实体：消息记录、模板、Provider 配置                                      │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Infrastructure Layer                                  │
│  Repository  ───  EF Core DbContext  ───  PostgreSQL                        │
│  数据持久化基础设施                                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 消息发送流程

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ 1. API 请求                                                                   │
│    POST /api/message/send                                                    │
│    → MessageController                                                       │
└───────────────────────────────┬──────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ 2. Grain 调用                                                                 │
│    → MessageGrain.SendAsync()                                                │
│    → 创建 MessageRecord，插入数据库                                             │
└───────────────────────────────┬──────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ 3. 模板渲染 (可选)                                                             │
│    → 根据 templateCode 查询模板                                                │
│    → 渲染 {{variable}} 占位符                                                  │
└───────────────────────────────┬──────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ 4. Provider 路由                                                              │
│    → SmsSenderFactory / PushSenderFactory                                    │
│    → 根据 Provider 名称或默认配置选择 Provider                                   │
└───────────────────────────────┬──────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ 5. Provider 发送                                                              │
│    → AliyunSmsSender (SDK)                                                   │
│    → TencentSmsSender (SDK)                                                  │
│    → HuaweiSmsSender (HTTP API)                                              │
│    → CtyunSmsSender (HTTP API)                                               │
│    → SmtpEmailSender (MailKit)                                               │
└───────────────────────────────┬──────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ 6. 结果更新                                                                   │
│    → 更新 MessageRecord 状态 (Sent/Failed)                                    │
│    → 记录外部消息 ID 或失败原因                                                  │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 设计模式

| 模式 | 实现 | 用途 |
|------|------|------|
| Strategy | `ISmsSender`, `IEmailSender`, `IPushSender` | 可互换的 Provider 算法 |
| Factory | `SmsSenderFactory`, `PushSenderFactory` | Provider 实例化 |
| Dependency Injection | Grain 构造函数注入 | 松耦合 |
| Repository | `IRepository<T, K>` | 数据访问抽象 |
| DTO/Projection | `MessageRecordDto`, `SendMessageInputDto` | API 契约 |

### Orleans 特性使用

| 特性 | 使用位置 | 用途 |
|------|----------|------|
| `[StatelessWorker]` | `MessageGrain`, `TemplateGrain` | 无状态 Grain，可在任意 Silo 运行 |
| `IGrainWithGuidKey` | `IMessageGrain`, `ITemplateGrain` | Grain 身份标识类型 |
| AdoNet Grain Storage | PostgreSQL | Grain 状态持久化 |
| Redis Clustering | `appsettings.json` | 分布式 Grain 激活 |

### 配置层次结构

```
appsettings.json
└── MessageSettings
    ├── EmailSettings
    │   ├── DefaultProvider: "smtp"
    │   └── SmtpSettings (Host, Port, Username, Password, From)
    │
    ├── SmsSettings
    │   ├── DefaultProvider: "aliyun"
    │   ├── AliyunSmsSettings (AccessKeyId, AccessKeySecret, SignName)
    │   ├── TencentSmsSettings (SecretId, SecretKey, SdkAppId, Region)
    │   ├── HuaweiSmsSettings (Ak, Sk, Sender, Endpoint)
    │   └── CtyunSmsSettings (AccessKeyId, AccessKeySecret, Signature)
    │
    └── PushSettings
        ├── DefaultProvider: "jpush"
        ├── JPushSettings (AppKey, MasterSecret)
        └── UmengSettings (AppKey, AppMasterSecret)
```

### Provider 实现对比

| Provider | 类型 | SDK/API | 认证方式 |
|----------|------|---------|----------|
| SMTP | Email | MailKit | Username/Password |
| 阿里云 | SMS | AlibabaCloud SDK | AccessKey |
| 腾讯云 | SMS | TencentCloud SDK 3.0 | SecretId/SecretKey |
| 华为云 | SMS | HTTP API | WSSE |
| 天翼云 | SMS | HTTP API | HMAC-SHA256 |
| 极光推送 | Push | - | - (预留) |
| 友盟推送 | Push | - | - (预留) |

## 支持的消息通道

| 通道 | Provider | 状态 | SDK/实现方式 |
|------|----------|------|--------------|
| Email | SMTP | ✅ 已实现 | MailKit |
| SMS | 阿里云 | ✅ 已实现 | AlibabaCloud SDK |
| SMS | 腾讯云 | ✅ 已实现 | TencentCloud SDK 3.0 |
| SMS | 华为云 | ✅ 已实现 | HTTP API (WSSE认证) |
| SMS | 天翼云 | ✅ 已实现 | HTTP API |
| Push | 极光推送 | 🔧 预留接口 | - |
| Push | 友盟推送 | 🔧 预留接口 | - |

## API 接口

### 消息接口 `/api/message`

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `/api/message/send` | 发送单条消息 |
| POST | `/api/message/batch-send` | 批量发送消息 |
| GET | `/api/message/{id}` | 获取消息记录 |
| GET | `/api/message` | 查询消息记录列表 |
| POST | `/api/message/{id}/retry` | 重试发送失败的消息 |
| POST | `/api/message/{id}/cancel` | 取消定时发送的消息 |

### 模板接口 `/api/template`

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `/api/template` | 创建模板 |
| PUT | `/api/template` | 更新模板 |
| DELETE | `/api/template/{id}` | 删除模板 |
| GET | `/api/template/{id}` | 获取模板详情 |
| GET | `/api/template/code/{code}` | 根据代码获取模板 |
| GET | `/api/template/channel/{channel}` | 根据通道获取模板列表 |
| GET | `/api/template` | 查询模板列表 |
| POST | `/api/template/{id}/preview` | 预览模板渲染结果 |

## 数据模型

### SendMessageInputDto（发送消息）

```json
{
  "channel": 1,           // 必填：1=Email, 2=SMS, 3=Push
  "receiver": "user@example.com",  // 必填：邮箱/手机号/设备Token
  "subject": "邮件主题",    // 选填：仅 Email 通道
  "content": "消息内容",   // 必填（无模板时）
  "templateCode": "tpl_001",  // 选填：模板代码
  "variables": {"name": "张三"},  // 选填：模板变量
  "provider": "smtp",      // 选填：指定 Provider
  "scheduledAt": "2026-03-21T10:00:00Z",  // 选填：定时发送时间
  "senderId": "uuid",      // 选填：发送者ID
  "businessId": "order_001",  // 选填：业务ID
  "businessType": "notification"  // 选填：业务类型
}
```

### BatchSendMessageInputDto（批量发送）

```json
{
  "channel": 1,
  "receivers": ["user1@example.com", "user2@example.com"],  // 必填：接收者列表
  "subject": "邮件主题",
  "content": "消息内容",
  "templateCode": "tpl_001",
  "variables": {"name": "用户"},
  "provider": "smtp",
  "senderId": "uuid",
  "businessId": "order_001",
  "businessType": "notification"
}
```

### CreateTemplateInputDto（创建模板）

```json
{
  "name": "注册验证码模板",
  "code": "register_verify_code",
  "channel": 2,
  "subjectTemplate": "您的验证码是 {{code}}",
  "contentTemplate": "您好 {{name}}，您的验证码是 {{code}}，{{minutes}}分钟内有效。",
  "variables": [
    {"name": "name", "type": "string", "required": true},
    {"name": "code", "type": "string", "required": true},
    {"name": "minutes", "type": "string", "required": false, "defaultValue": "5"}
  ],
  "description": "用于发送注册验证码",
  "defaultProvider": "aliyun",
  "tags": "auth,verify"
}
```

## 响应格式

所有 API 响应遵循统一格式：

```json
{
  "code": "success",
  "message": "success",
  "data": { ... }
}
```

错误响应：

```json
{
  "code": "invalid_receiver",
  "message": "Receiver is required.",
  "data": null
}
```

## 配置说明

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Message": "server=127.0.0.1;port=5432;Database=orleans;uid=postgres;pwd=123456",
    "Redis": "127.0.0.1:6379"
  },
  "Message": {
    "Email": {
      "DefaultProvider": "smtp",
      "Smtp": {
        "Host": "smtp.example.com",
        "Port": 587,
        "UseSsl": true,
        "Username": "noreply@example.com",
        "Password": "your-password",
        "From": "noreply@example.com",
        "FromName": "Stargazer"
      }
    },
    "Sms": {
      "DefaultProvider": "aliyun",
      "DefaultTemplateCode": "",
      "Aliyun": {
        "AccessKeyId": "your-access-key-id",
        "AccessKeySecret": "your-access-key-secret",
        "SignName": "签名名称",
        "Endpoint": "dysmsapi.aliyuncs.com"
      },
      "Tencent": {
        "SecretId": "your-secret-id",
        "SecretKey": "your-secret-key",
        "SdkAppId": "your-sdk-app-id",
        "TemplateId": "your-template-id",
        "SmsSign": "签名内容",
        "Region": "ap-guangzhou"
      },
      "Huawei": {
        "Ak": "your-access-key",
        "Sk": "your-secret-key",
        "Sender": "短信签名通道号",
        "Endpoint": "https://msgsms.cn-north-4.myhuaweicloud.com"
      },
      "Ctyun": {
        "AccessKeyId": "your-access-key-id",
        "AccessKeySecret": "your-access-key-secret",
        "Signature": "签名名称",
        "RequestUrl": "https://sms-global.ctapi.ctyun.cn/sms/api/v1"
      }
    },
    "Push": {
      "DefaultProvider": "jpush",
      "JPush": {
        "AppKey": "",
        "MasterSecret": ""
      },
      "Umeng": {
        "AppKey": "",
        "AppMasterSecret": ""
      }
    }
  }
}
```

## 数据库表结构

### msg_records（消息记录表）

| 字段 | 类型 | 说明 |
|------|------|------|
| id | uuid | 主键 |
| channel | int | 通道类型 |
| receiver | varchar(500) | 接收者 |
| subject | varchar(500) | 主题（邮件） |
| content | text | 消息内容 |
| variables | jsonb | 模板变量 |
| provider | varchar(50) | 使用的 Provider |
| status | int | 状态 |
| external_id | varchar(200) | 外部消息ID |
| failure_reason | text | 失败原因 |
| retry_count | int | 重试次数 |
| scheduled_at | timestamptz | 定时发送时间 |
| sent_at | timestamptz | 发送时间 |
| creation_time | timestamptz | 创建时间 |

### msg_templates（模板表）

| 字段 | 类型 | 说明 |
|------|------|------|
| id | uuid | 主键 |
| name | varchar(200) | 模板名称 |
| code | varchar(100) | 模板代码（唯一） |
| channel | int | 通道类型 |
| subject_template | varchar(500) | 主题模板 |
| content_template | text | 内容模板 |
| variables | jsonb | 变量定义 |
| is_active | bool | 是否启用 |
| version | int | 版本号 |
| creation_time | timestamptz | 创建时间 |

### msg_provider_configs（Provider 配置表）

| 字段 | 类型 | 说明 |
|------|------|------|
| id | uuid | 主键 |
| name | varchar(50) | Provider 名称 |
| channel | int | 通道类型 |
| config_json | text | 配置 JSON |
| is_enabled | bool | 是否启用 |
| priority | int | 优先级 |
| max_qps | int | 最大 QPS |
| is_healthy | bool | 健康状态 |

## 消息状态

| 值 | 状态 | 说明 |
|----|------|------|
| 0 | Pending | 等待发送 |
| 1 | Sending | 发送中 |
| 2 | Sent | 已发送 |
| 3 | Delivered | 已送达 |
| 4 | Failed | 发送失败 |
| 5 | Cancelled | 已取消 |

## 消息通道

| 值 | 通道 | 说明 |
|----|------|------|
| 1 | Email | 邮件 |
| 2 | Sms | 短信 |
| 3 | Push | 推送通知 |

## 使用示例

### 1. 发送邮件

```http
POST /api/message/send
Content-Type: application/json

{
  "channel": 1,
  "receiver": "user@example.com",
  "subject": "欢迎注册",
  "content": "欢迎 {{name}} 加入我们！"
}
```

### 2. 使用模板发送短信

```http
POST /api/message/send
Content-Type: application/json

{
  "channel": 2,
  "receiver": "13800138000",
  "templateCode": "sms_verify_code",
  "variables": {
    "code": "123456",
    "minutes": "5"
  },
  "provider": "aliyun"
}
```

### 3. 批量发送

```http
POST /api/message/batch-send
Content-Type: application/json

{
  "channel": 1,
  "receivers": ["user1@example.com", "user2@example.com"],
  "subject": "系统通知",
  "content": "您的订单 {{orderId}} 已发货"
}
```

### 4. 创建模板

```http
POST /api/template
Content-Type: application/json

{
  "name": "订单发货通知",
  "code": "order_shipped",
  "channel": 1,
  "subjectTemplate": "订单 {{orderId}} 已发货",
  "contentTemplate": "您好 {{customerName}}，您的订单 {{orderId}} 已于 {{shipTime}} 发货，预计 {{deliveryDays}} 天送达。",
  "variables": [
    {"name": "customerName", "type": "string", "required": true},
    {"name": "orderId", "type": "string", "required": true},
    {"name": "shipTime", "type": "string", "required": true},
    {"name": "deliveryDays", "type": "string", "required": false, "defaultValue": "3-5"}
  ]
}
```

### 5. 预览模板

```http
POST /api/template/{templateId}/preview
Content-Type: application/json

{
  "customerName": "张三",
  "orderId": "ORDER20260320001",
  "shipTime": "2026-03-20",
  "deliveryDays": "3-5"
}
```

## 开发指南

### 添加新的 SMS Provider

1. 在 `Senders/Sms/` 目录下创建新的 Sender 类，实现 `ISmsSender` 接口
2. 在 `SmsSenderFactory.cs` 中添加 Provider 路由
3. 在 `MessageSettings.cs` 中添加对应的配置类
4. 在 `appsettings.json` 中添加配置项

示例（SDK 方式）：

```csharp
public class NewSmsSender : ISmsSender
{
    public string ProviderName => "newprovider";
    
    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        // 实现发送逻辑
    }
    
    // 其他方法...
}
```

示例（HTTP API 方式）：

参考 `HuaweiSmsSender.cs` 或 `CtyunSmsSender.cs`，使用 HttpClient 调用第三方 HTTP API。

## SMS Provider 详解

### 阿里云短信

- 使用 AlibabaCloud SDK
- 需要配置：AccessKeyId、AccessKeySecret、SignName（签名名称）
- Endpoint 默认：`dysmsapi.aliyuncs.com`

### 腾讯云短信

- 使用 TencentCloud SDK 3.0
- 需要配置：SecretId、SecretKey、SdkAppId、TemplateId、SmsSign
- Region 默认：`ap-guangzhou`

### 华为云短信

- 使用 HTTP API，WSSE 认证
- 需要配置：Ak（Access Key）、Sk（Secret Key）、Sender（短信签名通道号）
- Endpoint 默认：`https://msgsms.cn-north-4.myhuaweicloud.com`

### 天翼云短信

- 使用 HTTP API，HMAC-SHA256 签名认证
- 需要配置：AccessKeyId、AccessKeySecret、Signature（签名名称）
- RequestUrl 默认：`https://sms-global.ctapi.ctyun.cn/sms/api/v1`

### 手机号格式

所有 SMS Provider 支持以下手机号格式：
- `13800138000` → 自动转换为 `+8613800138000`
- `8613800138000` → 自动转换为 `+8613800138000`
- `+8613800138000` → 保持不变

### 消息模板变量渲染

模板使用 `{{variable}}` 语法定义变量：

```
您好 {{name}}，您的验证码是 {{code}}，{{minutes}}分钟内有效。
```

渲染时传入变量：

```json
{
  "name": "张三",
  "code": "123456",
  "minutes": "5"
}
```

结果：

```
您好 张三，您的验证码是 123456，5分钟内有效。
```

## 注意事项

### 已实现功能

1. **SMS Provider**：所有 SMS Provider（阿里云、腾讯云、华为云、天翼云）均已实现
2. **Email Provider**：SMTP 邮件发送已实现（MailKit）
3. **模板管理**：支持 `{{variable}}` 占位符渲染
4. **批量发送**：支持批量发送消息
5. **消息记录**：完整的发送历史记录

### 待实现功能

1. **Push Provider**：极光推送和友盟推送接口预留，待实现
2. **定时消息消费**：定时消息需要在定时时间到达后被消费，需配合后台调度器
3. **自动重试**：目前仅支持手动重试，可扩展自动重试机制
4. **Provider 健康检查**：暂无自动故障转移
5. **限流控制**：暂无内置限流

### 技术选型说明

1. **华为云 SMS**：使用 HTTP API（WSSE 认证），而非 SDK（SDK 仅提供管理 API）
2. **天翼云 SMS**：使用 HTTP API（HMAC-SHA256 认证）
3. **手机号处理**：所有 SMS Provider 自动处理中国手机号格式（支持 `138xxxx`、`+86138xxx`、`86138xxx`）
