using Orleans;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UserProfileDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Account { get; set; } = "";

    [Id(2)]
    public string Name { get; set; } = "";

    [Id(3)]
    public string Email { get; set; } = "";

    [Id(4)]
    public string PhoneNumber { get; set; } = "";

    [Id(5)]
    public string Avatar { get; set; } = "";

    [Id(6)]
    public bool IsActive { get; set; } = true;
    
    [Id(7)]
    public List<RoleDataDto> Roles { get; set; } = new();
    
    [Id(8)]
    public DateTime CreationTime { get; set; }
}
