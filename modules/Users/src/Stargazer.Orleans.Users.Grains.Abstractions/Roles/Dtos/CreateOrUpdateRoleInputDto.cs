using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;

[GenerateSerializer]
public class CreateOrUpdateRoleInputDto
{
    [Id(0)]
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers and underscores")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(1)]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [Id(2)]
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }

    [Id(3)]
    [Range(0, int.MaxValue, ErrorMessage = "Priority must be a non-negative number")]
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [Id(4)]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [Id(5)]
    [JsonPropertyName("permission_ids")]
    public List<Guid> PermissionIds { get; set; } = new();
}
