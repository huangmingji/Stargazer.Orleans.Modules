using Orleans;

namespace Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class VerifyPasswordInputDto
{
    [Id(0)]
    public string Password { get; set; } = "";

    [Id(1)]
    public string Name { get; set; } = "";
}