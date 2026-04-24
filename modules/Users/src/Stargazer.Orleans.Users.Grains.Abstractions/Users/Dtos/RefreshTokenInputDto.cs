using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RefreshTokenInputDto
{
    [Id(0)]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";
}
