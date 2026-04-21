using Microsoft.AspNetCore.Authorization;
using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;

namespace Stargazer.Orleans.WechatManagement.Silo.Authorization;

public class WechatPermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public WechatPermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}

public class WechatPermissionHandler : AuthorizationHandler<WechatPermissionRequirement>
{
    private readonly IClusterClient _clusterClient;

    public WechatPermissionHandler(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WechatPermissionRequirement requirement)
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

        var userGrain = _clusterClient.GetGrain<IUserGrain>(0);
        var hasPermission = await userGrain.HasPermissionAsync(userId, requirement.PermissionCode);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

public static class WechatPolicyNames
{
    public const string ViewAccounts = "permission:" + AuthorizationPermissions.Accounts.View;
    public const string CreateAccounts = "permission:" + AuthorizationPermissions.Accounts.Create;
    public const string UpdateAccounts = "permission:" + AuthorizationPermissions.Accounts.Update;
    public const string DeleteAccounts = "permission:" + AuthorizationPermissions.Accounts.Delete;

    public const string ViewFans = "permission:" + AuthorizationPermissions.Fans.View;
    public const string UpdateFans = "permission:" + AuthorizationPermissions.Fans.Update;
    public const string TagFans = "permission:" + AuthorizationPermissions.Fans.Tag;

    public const string ViewGroups = "permission:" + AuthorizationPermissions.Groups.View;
    public const string CreateGroups = "permission:" + AuthorizationPermissions.Groups.Create;
    public const string UpdateGroups = "permission:" + AuthorizationPermissions.Groups.Update;
    public const string DeleteGroups = "permission:" + AuthorizationPermissions.Groups.Delete;

    public const string ViewTags = "permission:" + AuthorizationPermissions.Tags.View;
    public const string CreateTags = "permission:" + AuthorizationPermissions.Tags.Create;
    public const string UpdateTags = "permission:" + AuthorizationPermissions.Tags.Update;
    public const string DeleteTags = "permission:" + AuthorizationPermissions.Tags.Delete;

    public const string SendTemplateMessage = "permission:" + AuthorizationPermissions.Messages.SendTemplate;
    public const string SendCustomMessage = "permission:" + AuthorizationPermissions.Messages.SendCustom;
    public const string SendMassMessage = "permission:" + AuthorizationPermissions.Messages.SendMass;
}
