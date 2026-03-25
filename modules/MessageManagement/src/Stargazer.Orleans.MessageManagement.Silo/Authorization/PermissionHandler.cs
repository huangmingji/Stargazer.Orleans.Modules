using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

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
    private readonly HttpClient _httpClient;
    private readonly string _usersServiceBaseUrl;

    public PermissionHandler(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _usersServiceBaseUrl = configuration["UsersService:BaseUrl"] ?? "http://localhost:5079";
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
            var request = new { UserId = userId, Permission = requirement.Permission };
            var response = await _httpClient.PostAsJsonAsync($"{_usersServiceBaseUrl}/api/user/has-permission", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PermissionCheckResult>();
                if (result?.Success == true && result.Data == true)
                {
                    context.Succeed(requirement);
                }
            }
        }
        catch
        {
            // If the HTTP call fails, deny access by default
        }
    }
}

public class PermissionCheckResult
{
    public bool Success { get; set; }
    public bool? Data { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
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
