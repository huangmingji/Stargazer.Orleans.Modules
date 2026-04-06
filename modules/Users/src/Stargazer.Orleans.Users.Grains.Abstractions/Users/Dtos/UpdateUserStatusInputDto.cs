using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UpdateUserStatusInputDto
{
    [Id(0)]
    public bool IsEnabled { get; set; }
}
