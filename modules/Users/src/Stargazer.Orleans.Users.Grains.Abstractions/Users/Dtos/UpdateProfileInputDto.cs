using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Orleans;

namespace Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

[GenerateSerializer]
public class UpdateProfileInputDto
{
    [Id(0)]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [Id(1)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [Id(2)]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = "";

    [Id(3)]
    [Url(ErrorMessage = "Invalid avatar URL format")]
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = "";
}
