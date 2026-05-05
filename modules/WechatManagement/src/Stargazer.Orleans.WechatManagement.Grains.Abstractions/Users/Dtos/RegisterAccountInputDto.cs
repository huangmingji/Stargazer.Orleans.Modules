using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RegisterAccountInputDto
{
    [Id(0)]
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; } = "";

    [Id(1)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
}
