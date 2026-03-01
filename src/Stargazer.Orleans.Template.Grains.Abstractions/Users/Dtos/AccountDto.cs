using Orleans;

namespace Stargazer.Orleans.Template.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class AccountDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string AccountName { get; set; }

    [Id(2)]
    public string Password { get; set; }

    [Id(3)]
    public string SaltKey { get; set; }

    [Id(4)]
    public Guid CreatorId { get; set; }

    [Id(5)]
    public DateTime CreateTime { get; set; }

    [Id(6)]
    public Guid? LastModifierId { get; set; }

    [Id(7)]
    public DateTime? LastModifyTime { get; set; }
}