namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

public class WechatUserDto
{
    public Guid Id { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string? UnionId { get; set; }
    public Guid AccountId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Sex { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? HeadimgUrl { get; set; }
    public DateTime? SubscribeTime { get; set; }
    public DateTime? UnSubscribeTime { get; set; }
    public string Remark { get; set; } = string.Empty;
    public int SubscribeStatus { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public List<WechatUserTagDto> Tags { get; set; } = new();
    public DateTime CreationTime { get; set; }
}

public class CreateWechatUserInputDto
{
    public string OpenId { get; set; } = string.Empty;
    public string? UnionId { get; set; }
    public Guid AccountId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Sex { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? HeadimgUrl { get; set; }
    public DateTime? SubscribeTime { get; set; }
    public Guid? GroupId { get; set; }
}

public class UpdateWechatUserInputDto
{
    public string? Remark { get; set; }
    public Guid? GroupId { get; set; }
}

public class WechatUserTagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WechatTagId { get; set; }
    public int UserCount { get; set; }
}

public class WechatUserGroupDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WechatGroupId { get; set; }
    public int UserCount { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateWechatUserGroupInputDto
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateWechatUserGroupInputDto
{
    public string? Name { get; set; }
}

public class CreateWechatUserTagInputDto
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateWechatUserTagInputDto
{
    public string? Name { get; set; }
}
