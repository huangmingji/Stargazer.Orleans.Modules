using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class CreateOrUpdateUserInputDto
{
    [Id(0)]
    public string Account { get; set; } = "";
    
    [Id(1)]
    public string Password { get; set; } = "";
    
    [Id(2)]
    public string Name { get; set; } = "";

    [Id(3)]
    public string Email { get; set; } = "";

    [Id(4)]
    public string PhoneNumber { get; set; } = "";

    [Id(5)]
    public string Avatar { get; set; } = "";
}
