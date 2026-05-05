using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class ChangePasswordInputDto
{
    [Id(0)]
    [JsonPropertyName("old_password")]
    public string OldPassword { get; set; } = "";

    [Id(1)]
    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = "";
}
