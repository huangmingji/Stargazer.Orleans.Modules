using Orleans;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class CreateOrUpdateUserInputDto
{
    [Id(0)]
    public string UserName { get; set; } = "";

    [Id(1)]
    public string Email { get; set; } = "";

    [Id(2)]
    public string PhoneNumber { get; set; } = "";

    [Id(3)]
    public string Avatar { get; set; } = "";
}
