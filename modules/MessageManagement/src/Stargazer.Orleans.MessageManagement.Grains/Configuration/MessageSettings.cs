namespace Stargazer.Orleans.MessageManagement.Grains.Configuration;

/// <summary>
/// 消息配置根类
/// </summary>
public class MessageSettings
{
    public EmailSettings Email { get; set; } = new();
    public SmsSettings Sms { get; set; } = new();
    public PushSettings Push { get; set; } = new();
}

/// <summary>
/// Email配置
/// </summary>
public class EmailSettings
{
    public string DefaultProvider { get; set; } = "smtp";
    public SmtpSettings? Smtp { get; set; }
}

/// <summary>
/// SMTP配置
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

/// <summary>
/// SMS配置
/// </summary>
public class SmsSettings
{
    public string DefaultProvider { get; set; } = "aliyun";
    public string DefaultTemplateCode { get; set; } = string.Empty;
    public AliyunSmsSettings? Aliyun { get; set; }
    public TencentSmsSettings? Tencent { get; set; }
    public HuaweiSmsSettings? Huawei { get; set; }
    public CtyunSmsSettings? Ctyun { get; set; }
}

/// <summary>
/// 阿里云短信配置
/// </summary>
public class AliyunSmsSettings
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string SignName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "dysmsapi.aliyuncs.com";
}

/// <summary>
/// 腾讯云短信配置
/// </summary>
public class TencentSmsSettings
{
    public string SecretId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string SdkAppId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string SmsSign { get; set; } = string.Empty;
    public string Region { get; set; } = "ap-guangzhou";
}

/// <summary>
/// 华为云短信配置
/// </summary>
public class HuaweiSmsSettings
{
    public string Ak { get; set; } = string.Empty;
    public string Sk { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://msgsms.cn-north-4.myhuaweicloud.com";
}

/// <summary>
/// 天翼云短信配置
/// </summary>
public class CtyunSmsSettings
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = "https://sms-global.ctapi.ctyun.cn/sms/api/v1";
}

/// <summary>
/// Push配置
/// </summary>
public class PushSettings
{
    public string DefaultProvider { get; set; } = "jpush";
    public JPushSettings? JPush { get; set; }
    public UmengSettings? Umeng { get; set; }
}

/// <summary>
/// 极光推送配置
/// </summary>
public class JPushSettings
{
    public string AppKey { get; set; } = string.Empty;
    public string MasterSecret { get; set; } = string.Empty;
}

/// <summary>
/// 友盟推送配置
/// </summary>
public class UmengSettings
{
    public string AppKey { get; set; } = string.Empty;
    public string AppMasterSecret { get; set; } = string.Empty;
}
