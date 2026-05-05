using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class CreateOrUpdateUserInputDto
{
    [Id(0)]
    [JsonPropertyName("user_name")]
    public string UserName { get; set; } = "";

    [Id(1)]
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [Id(2)]
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = "";

    [Id(3)]
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "";
}
