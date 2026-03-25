using Microsoft.AspNetCore.Authorization;
using IUserGrain = Stargazer.Orleans.Users.Grains.Abstractions.Users.IUserGrain;

namespace Stargazer.Orleans.MessageManagement.Silo.Authorization;

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
        if (user.Identity?.IsAuthenticated != true)
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
        options.AddPolicy("permission:message.send", policy =>
            policy.Requirements.Add(new PermissionRequirement("message.send")));
        
        options.AddPolicy("permission:message.view", policy =>
            policy.Requirements.Add(new PermissionRequirement("message.view")));
        
        options.AddPolicy("permission:message.retry", policy =>
            policy.Requirements.Add(new PermissionRequirement("message.retry")));
        
        options.AddPolicy("permission:message.cancel", policy =>
            policy.Requirements.Add(new PermissionRequirement("message.cancel")));
        
        options.AddPolicy("permission:template.view", policy =>
            policy.Requirements.Add(new PermissionRequirement("template.view")));
        
        options.AddPolicy("permission:template.create", policy =>
            policy.Requirements.Add(new PermissionRequirement("template.create")));
        
        options.AddPolicy("permission:template.update", policy =>
            policy.Requirements.Add(new PermissionRequirement("template.update")));
        
        options.AddPolicy("permission:template.delete", policy =>
            policy.Requirements.Add(new PermissionRequirement("template.delete")));

        return options;
    }
}
