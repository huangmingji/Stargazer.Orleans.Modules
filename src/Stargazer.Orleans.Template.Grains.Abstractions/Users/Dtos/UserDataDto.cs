using Orleans;

namespace Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UserDataDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public Guid? AccountId { get; set; }

    [Id(2)]
    public string UserName { get; set; }

    [Id(3)]
    public string Email { get; set; }

    [Id(4)]
    public string PhoneNumber { get; set; }

    [Id(5)]
    public string Avatar { get; set; }

    [Id(6)]
    public Guid CreatorId { get; set; }

    [Id(7)]
    public DateTime CreateTime { get; set; }

    [Id(8)]
    public Guid? LastModifierId { get; set; }

    [Id(9)]
    public DateTime? LastModifyTime { get; set; }
}