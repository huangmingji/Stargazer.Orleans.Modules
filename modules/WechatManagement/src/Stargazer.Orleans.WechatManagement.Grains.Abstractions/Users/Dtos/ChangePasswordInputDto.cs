using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class ChangePasswordInputDto
{
    [Id(0)]
    public string OldPassword { get; set; } = "";

    [Id(1)]
    public string NewPassword { get; set; } = "";
}
