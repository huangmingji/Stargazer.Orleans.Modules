using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UserDataDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Account { get; set; }

    [Id(2)]
    public string Name { get; set; }

    [Id(3)]
    public string Email { get; set; }

    [Id(4)]
    public string PhoneNumber { get; set; }

    [Id(5)]
    public string Avatar { get; set; }

    [Id(6)]
    public Guid CreatorId { get; set; }

    [Id(7)]
    public DateTime CreationTime { get; set; }

    [Id(8)]
    public Guid? LastModifierId { get; set; }

    [Id(9)]
    public DateTime? LastModifyTime { get; set; }
}
