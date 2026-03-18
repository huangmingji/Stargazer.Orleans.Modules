using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class PermissionDataDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Name { get; set; } = "";

    [Id(2)]
    public string Code { get; set; } = "";

    [Id(3)]
    public string Description { get; set; } = "";

    [Id(4)]
    public string Category { get; set; } = "";

    [Id(5)]
    public int Type { get; set; }

    [Id(6)]
    public bool IsActive { get; set; } = true;
}
