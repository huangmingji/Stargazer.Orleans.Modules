using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class TokenResponseDto
{
    [Id(0)]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";
    
    [Id(1)]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";
    
    [Id(2)]
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
    
    [Id(3)]
    [JsonPropertyName("user")]
    public UserDataDto User { get; set; } = new();
}
