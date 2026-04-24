using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class CreateOrUpdateUserInputDto
{
    [Id(0)]
    [Required(ErrorMessage = "Account is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Account must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Account can only contain letters, numbers and underscores")]
    [JsonPropertyName("account")]
    public string Account { get; set; } = "";
    
    [Id(1)]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
    
    [Id(2)]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(3)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [Id(4)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = "";

    [Id(5)]
    [Url(ErrorMessage = "Invalid avatar URL format")]
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "";
    
    [Id(6)]
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Id(7)]
    [JsonPropertyName("role")]
    public string? Role { get; set; }
}
