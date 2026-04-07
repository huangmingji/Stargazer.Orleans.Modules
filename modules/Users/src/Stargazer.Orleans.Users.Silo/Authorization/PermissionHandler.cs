using Microsoft.AspNetCore.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.Users.Silo.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IClusterClient _client;

    public PermissionHandler(IClusterClient client)
    {
        _client = client;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var userIdClaim = user.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }
        
        var userGrain = _client.GetGrain<IUserGrain>(0);
        var hasPermission = await userGrain.HasPermissionAsync(userId, requirement.Permission, CancellationToken.None);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

public static class PermissionAuthorizationExtensions
{
    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy($"permission:{AuthorizationPermissions.Roles.View}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Roles.View)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Roles.Create}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Roles.Create)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Roles.Update}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Roles.Update)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Roles.Delete}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Roles.Delete)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Roles.Assign}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Roles.Assign)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Users.View}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Users.View)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Users.Create}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Users.Create)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Users.Update}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Users.Update)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Users.Delete}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Users.Delete)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Users.Assign}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Users.Assign)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Permissions.View}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Permissions.View)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Permissions.Create}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Permissions.Create)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Permissions.Update}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Permissions.Update)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Permissions.Delete}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Permissions.Delete)));

        return options;
    }
}
