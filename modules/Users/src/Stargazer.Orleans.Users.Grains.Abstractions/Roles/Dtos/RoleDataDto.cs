using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class RoleDataDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Name { get; set; } = "";

    [Id(2)]
    public string Description { get; set; } = "";

    [Id(3)]
    public bool IsDefault { get; set; }

    [Id(4)]
    public int Priority { get; set; }

    [Id(5)]
    public bool IsActive { get; set; } = true;

    [Id(6)]
    public List<PermissionDataDto> Permissions { get; set; } = new();

    [Id(7)]
    public DateTime CreationTime { get; set; }
}
