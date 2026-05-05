using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class AccountDto
{
    [Id(0)]
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)]
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; }

    [Id(2)]
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [Id(3)]
    [JsonPropertyName("salt_key")]
    public string SaltKey { get; set; }

    [Id(4)]
    [JsonPropertyName("creator_id")]
    public Guid CreatorId { get; set; }

    [Id(5)]
    [JsonPropertyName("create_time")]
    public DateTime CreateTime { get; set; }

    [Id(6)]
    [JsonPropertyName("last_modifier_id")]
    public Guid? LastModifierId { get; set; }

    [Id(7)]
    [JsonPropertyName("last_modify_time")]
    public DateTime? LastModifyTime { get; set; }
}
