namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts.Dtos;

public class WechatAccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string EncodingAESKey { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime? AccessTokenExpiry { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastModifyTime { get; set; }
}

public class CreateWechatAccountInputDto
{
    public string Name { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string EncodingAESKey { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class UpdateWechatAccountInputDto
{
    public string? Name { get; set; }
    public string? AppSecret { get; set; }
    public string? Token { get; set; }
    public string? EncodingAESKey { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}
