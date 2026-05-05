using System.Text.Json.Serialization;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

public class WechatUserDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("union_id")]
    public string? UnionId { get; set; }

    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("sex")]
    public int Sex { get; set; }

    [JsonPropertyName("province")]
    public string? Province { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("headimg_url")]
    public string? HeadimgUrl { get; set; }

    [JsonPropertyName("subscribe_time")]
    public DateTime? SubscribeTime { get; set; }

    [JsonPropertyName("un_subscribe_time")]
    public DateTime? UnSubscribeTime { get; set; }

    [JsonPropertyName("remark")]
    public string Remark { get; set; } = string.Empty;

    [JsonPropertyName("subscribe_status")]
    public int SubscribeStatus { get; set; }

    [JsonPropertyName("group_id")]
    public Guid? GroupId { get; set; }

    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    [JsonPropertyName("tags")]
    public List<WechatUserTagDto> Tags { get; set; } = new();

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}

public class CreateWechatUserInputDto
{
    [JsonPropertyName("open_id")]
    public string OpenId { get; set; } = string.Empty;

    [JsonPropertyName("union_id")]
    public string? UnionId { get; set; }

    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; } = string.Empty;

    [JsonPropertyName("sex")]
    public int Sex { get; set; }

    [JsonPropertyName("province")]
    public string? Province { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("headimg_url")]
    public string? HeadimgUrl { get; set; }

    [JsonPropertyName("subscribe_time")]
    public DateTime? SubscribeTime { get; set; }

    [JsonPropertyName("group_id")]
    public Guid? GroupId { get; set; }
}

public class UpdateWechatUserInputDto
{
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }

    [JsonPropertyName("group_id")]
    public Guid? GroupId { get; set; }
}

public class WechatUserTagDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("wechat_tag_id")]
    public int WechatTagId { get; set; }

    [JsonPropertyName("user_count")]
    public int UserCount { get; set; }
}

public class WechatUserGroupDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("wechat_group_id")]
    public int WechatGroupId { get; set; }

    [JsonPropertyName("user_count")]
    public int UserCount { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }
}

public class CreateWechatUserGroupInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateWechatUserGroupInputDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class CreateWechatUserTagInputDto
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class UpdateWechatUserTagInputDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
