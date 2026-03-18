using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class TokenResponseDto
{
    [Id(0)]
    public string AccessToken { get; set; } = "";
    
    [Id(1)]
    public string RefreshToken { get; set; } = "";
    
    [Id(2)]
    public DateTime ExpiresAt { get; set; }
    
    [Id(3)]
    public UserDataDto User { get; set; } = new();
}
