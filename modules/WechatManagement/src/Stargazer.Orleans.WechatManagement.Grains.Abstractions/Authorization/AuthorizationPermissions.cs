namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Authorization;

public static class AuthorizationPermissions
{
    public static class Accounts
    {
        public const string View = "wechat.account.view";
        public const string Create = "wechat.account.create";
        public const string Update = "wechat.account.update";
        public const string Delete = "wechat.account.delete";
    }

    public static class Fans
    {
        public const string View = "wechat.fan.view";
        public const string Update = "wechat.fan.update";
        public const string Tag = "wechat.fan.tag";
    }

    public static class Groups
    {
        public const string View = "wechat.group.view";
        public const string Create = "wechat.group.create";
        public const string Update = "wechat.group.update";
        public const string Delete = "wechat.group.delete";
    }

    public static class Tags
    {
        public const string View = "wechat.tag.view";
        public const string Create = "wechat.tag.create";
        public const string Update = "wechat.tag.update";
        public const string Delete = "wechat.tag.delete";
    }

    public static class Messages
    {
        public const string SendTemplate = "wechat.message.template";
        public const string SendCustom = "wechat.message.custom";
        public const string SendMass = "wechat.message.mass";
    }
}
