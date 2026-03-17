using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RegisterAccountInputDto
{
    [Id(0)]
    public string Account { get; set; } = "";

    [Id(1)]
    public string Password { get; set; } = "";
}
