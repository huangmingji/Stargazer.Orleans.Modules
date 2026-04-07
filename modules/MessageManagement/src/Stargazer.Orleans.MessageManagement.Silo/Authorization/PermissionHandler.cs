using Microsoft.AspNetCore.Authorization;
using Orleans;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

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
    private readonly IClusterClient _clusterClient;

    public PermissionHandler(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
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

        try
        {
            var userGrain = _clusterClient.GetGrain<IUserGrain>(0);
            var hasPermission = await userGrain.HasPermissionAsync(userId, requirement.Permission);
            
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
        catch
        {
        }
    }
}

public static class PermissionAuthorizationExtensions
{
    public static AuthorizationOptions AddPermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy($"permission:{AuthorizationPermissions.Messages.Send}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Messages.Send)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Messages.View}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Messages.View)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Messages.Retry}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Messages.Retry)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Messages.Cancel}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Messages.Cancel)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Templates.View}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Templates.View)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Templates.Create}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Templates.Create)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Templates.Update}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Templates.Update)));
        
        options.AddPolicy($"permission:{AuthorizationPermissions.Templates.Delete}", policy =>
            policy.Requirements.Add(new PermissionRequirement(AuthorizationPermissions.Templates.Delete)));

        return options;
    }
}
