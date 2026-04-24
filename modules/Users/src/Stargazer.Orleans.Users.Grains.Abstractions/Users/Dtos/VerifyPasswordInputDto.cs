using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class VerifyPasswordInputDto
{
    [Id(0)]
    [JsonPropertyName("password")]
    public string Password { get; set; } = "";

    [Id(1)]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";
}
