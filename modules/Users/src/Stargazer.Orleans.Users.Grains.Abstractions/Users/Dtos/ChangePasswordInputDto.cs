using System.ComponentModel.DataAnnotations;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class ChangePasswordInputDto
{
    [Id(0)]
    [Required(ErrorMessage = "Old password is required")]
    public string OldPassword { get; set; } = "";

    [Id(1)]
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$", ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, and one digit")]
    public string NewPassword { get; set; } = "";
}
