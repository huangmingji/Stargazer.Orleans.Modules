using System.ComponentModel.DataAnnotations;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class CreateOrUpdateUserInputDto
{
    [Id(0)]
    [Required(ErrorMessage = "Account is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Account must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Account can only contain letters, numbers and underscores")]
    public string Account { get; set; } = "";
    
    [Id(1)]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = "";
    
    [Id(2)]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = "";

    [Id(3)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = "";

    [Id(4)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string PhoneNumber { get; set; } = "";

    [Id(5)]
    [Url(ErrorMessage = "Invalid avatar URL format")]
    public string Avatar { get; set; } = "";
    
    [Id(6)]
    public bool IsActive { get; set; } = true;
    
    [Id(7)]
    public string? Role { get; set; }
}
