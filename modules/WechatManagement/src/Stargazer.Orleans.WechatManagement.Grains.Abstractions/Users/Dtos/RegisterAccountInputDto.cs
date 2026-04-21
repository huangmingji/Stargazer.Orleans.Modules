using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RegisterAccountInputDto
{
    [Id(0)]
    public string AccountName { get; set; } = "";

    [Id(1)]
    public string Password { get; set; } = "";
}
