using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class CreateOrUpdateRoleInputDto
{
    [Id(0)]
    public string Name { get; set; } = "";

    [Id(1)]
    public string Description { get; set; } = "";

    [Id(2)]
    public bool IsDefault { get; set; }

    [Id(3)]
    public int Priority { get; set; }

    [Id(4)]
    public bool IsActive { get; set; } = true;

    [Id(5)]
    public List<Guid> PermissionIds { get; set; } = new();
}
