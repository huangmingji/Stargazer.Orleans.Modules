namespace Stargazer.Orleans.Users.Grains.Abstractions.Authorization;

public static class AuthorizationPermissions
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string Assign = "users.assign_role";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string Assign = "roles.assign_permission";
    }

    public static class Permissions
    {
        public const string View = "permissions.view";
        public const string Create = "permissions.create";
        public const string Update = "permissions.update";
        public const string Delete = "permissions.delete";
    }
}