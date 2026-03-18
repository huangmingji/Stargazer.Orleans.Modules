using System.ComponentModel.DataAnnotations;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class RegisterAccountInputDto
{
    [Id(0)]
    [Required(ErrorMessage = "Account is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Account must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Account can only contain letters, numbers and underscores")]
    public string Account { get; set; } = "";

    [Id(1)]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
    public string Password { get; set; } = "";
    
    [Id(2)]
    public string? Role { get; set; } = "User";
}
