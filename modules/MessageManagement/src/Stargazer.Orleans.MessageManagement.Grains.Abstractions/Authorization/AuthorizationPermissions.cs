namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;

public static class AuthorizationPermissions
{
    public static class Messages
    {
        public const string Send = "message.send";
        public const string View = "message.view";
        public const string Retry = "message.retry";
        public const string Cancel = "message.cancel";
    }
    
    public static class Templates
    {
        public const string View = "template.view";
        public const string Create = "template.create";
        public const string Update = "template.update";
        public const string Delete = "template.delete";
    }
}
