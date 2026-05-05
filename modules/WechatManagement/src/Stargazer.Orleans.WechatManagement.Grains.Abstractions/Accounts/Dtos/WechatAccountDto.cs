using System.Text.Json.Serialization;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts.Dtos;

public class WechatAccountDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("app_id")]
    public string AppId { get; set; } = string.Empty;

    [JsonPropertyName("app_secret")]
    public string AppSecret { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("encoding_aes_key")]
    public string EncodingAESKey { get; set; } = string.Empty;

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("access_token_expiry")]
    public DateTime? AccessTokenExpiry { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }

    [JsonPropertyName("last_modify_time")]
    public DateTime LastModifyTime { get; set; }
}

public class CreateWechatAccountInputDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("app_id")]
    public string AppId { get; set; } = string.Empty;

    [JsonPropertyName("app_secret")]
    public string AppSecret { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("encoding_aes_key")]
    public string EncodingAESKey { get; set; } = string.Empty;

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }
}

public class UpdateWechatAccountInputDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("app_secret")]
    public string? AppSecret { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("encoding_aes_key")]
    public string? EncodingAESKey { get; set; }

    [JsonPropertyName("is_default")]
    public bool? IsDefault { get; set; }

    [JsonPropertyName("is_active")]
    public bool? IsActive { get; set; }
}
