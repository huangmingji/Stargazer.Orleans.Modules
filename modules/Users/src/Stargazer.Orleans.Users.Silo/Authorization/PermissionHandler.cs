using Microsoft.AspNetCore.Authorization;
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
        options.AddPolicy("permission:role.view", policy =>
            policy.Requirements.Add(new PermissionRequirement("role.view")));
        
        options.AddPolicy("permission:role.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("role.create")));
        
        options.AddPolicy("permission:role.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("role.update")));
        
        options.AddPolicy("permission:role.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("role.delete")));
        
        options.AddPolicy("permission:role.assign", policy =>
            policy.Requirements.Add(new PermissionRequirement("role.assign")));
        
        options.AddPolicy("permission:user.view", policy =>
            policy.Requirements.Add(new PermissionRequirement("user.view")));
        
        options.AddPolicy("permission:user.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("user.create")));
        
        options.AddPolicy("permission:user.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("user.update")));
        
        options.AddPolicy("permission:user.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("user.delete")));
        
        options.AddPolicy("permission:user.assign", policy =>
            policy.Requirements.Add(new PermissionRequirement("user.assign")));
        
        options.AddPolicy("permission:permission.view", policy =>
            policy.Requirements.Add(new PermissionRequirement("permission.view")));
        
        options.AddPolicy("permission:permission.manage", policy =>
            policy.Requirements.Add(new PermissionRequirement("permission.manage")));

        return options;
    }
}
