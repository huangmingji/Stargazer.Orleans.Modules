using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RefreshTokenInputDto
{
    [Id(0)]
    public string RefreshToken { get; set; } = "";
}
