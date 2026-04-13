using Microsoft.AspNetCore.Authorization;
using Orleans;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.ObjectStorage.Silo.Authorization;

public class StoragePermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public StoragePermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class StoragePermissionHandler : AuthorizationHandler<StoragePermissionRequirement>
{
    private readonly IClusterClient _clusterClient;

    public StoragePermissionHandler(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoragePermissionRequirement requirement)
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

public static class StoragePermissionAuthorizationExtensions
{
    public static AuthorizationOptions AddStoragePermissionPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy($"permission:{StoragePolicies.Buckets.View}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Buckets.View)));
        
        options.AddPolicy($"permission:{StoragePolicies.Buckets.Create}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Buckets.Create)));
        
        options.AddPolicy($"permission:{StoragePolicies.Buckets.Update}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Buckets.Update)));
        
        options.AddPolicy($"permission:{StoragePolicies.Buckets.Delete}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Buckets.Delete)));
        
        options.AddPolicy($"permission:{StoragePolicies.Objects.View}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Objects.View)));
        
        options.AddPolicy($"permission:{StoragePolicies.Objects.Create}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Objects.Create)));
        
        options.AddPolicy($"permission:{StoragePolicies.Objects.Update}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Objects.Update)));
        
        options.AddPolicy($"permission:{StoragePolicies.Objects.Delete}", policy =>
            policy.Requirements.Add(new StoragePermissionRequirement(StoragePolicies.Objects.Delete)));

        return options;
    }
}
