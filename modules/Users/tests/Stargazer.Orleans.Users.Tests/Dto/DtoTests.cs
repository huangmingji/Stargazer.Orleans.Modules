using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;
using Stargazer.Orleans.Users.Grains.Abstractions.Roles.Dtos;
using Xunit;

namespace Stargazer.Orleans.Users.Tests.Dto;

public class ResponseDataTests
{
    [Fact]
    public void Success_ReturnsCorrectResponse()
    {
        var response = ResponseData.Success();

        Assert.Equal("success", response.Code);
        Assert.Equal("success", response.Message);
        Assert.Null(response.Data);
    }

    [Fact]
    public void Success_WithCustomValues()
    {
        var data = new { Name = "Test" };
        var response = ResponseData.Success("custom_code", "Custom message", data);

        Assert.Equal("custom_code", response.Code);
        Assert.Equal("Custom message", response.Message);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public void Fail_ReturnsCorrectResponse()
    {
        var response = ResponseData.Fail("error_code", "Error message");

        Assert.Equal("error_code", response.Code);
        Assert.Equal("Error message", response.Message);
        Assert.Null(response.Data);
    }

    [Fact]
    public void Fail_WithDefaultValues()
    {
        var response = ResponseData.Fail();

        Assert.Equal("fail", response.Code);
        Assert.Equal(string.Empty, response.Message);
    }
}

public class UserDataDtoTests
{
    [Fact]
    public void UserDataDto_CanSetProperties()
    {
        var dto = new UserDataDto
        {
            Id = Guid.NewGuid(),
            Account = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Avatar = "avatar.png",
            IsActive = true
        };

        Assert.Equal("testuser", dto.Account);
        Assert.Equal("Test User", dto.Name);
        Assert.Equal("test@example.com", dto.Email);
        Assert.Equal("1234567890", dto.PhoneNumber);
        Assert.Equal("avatar.png", dto.Avatar);
        Assert.True(dto.IsActive);
    }
}

public class UserProfileDtoTests
{
    [Fact]
    public void UserProfileDto_CanSetProperties()
    {
        var dto = new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Account = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            Avatar = "avatar.png",
            IsActive = true
        };

        Assert.Equal("testuser", dto.Account);
        Assert.Equal("Test User", dto.Name);
        Assert.Equal("test@example.com", dto.Email);
        Assert.Equal("1234567890", dto.PhoneNumber);
        Assert.True(dto.IsActive);
    }
}

public class TokenResponseDtoTests
{
    [Fact]
    public void TokenResponseDto_CanSetProperties()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var dto = new TokenResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = expiresAt,
            User = new UserDataDto { Id = userId, Account = "testuser" }
        };

        Assert.Equal("access_token", dto.AccessToken);
        Assert.Equal("refresh_token", dto.RefreshToken);
        Assert.Equal(expiresAt, dto.ExpiresAt);
        Assert.NotNull(dto.User);
        Assert.Equal("testuser", dto.User.Account);
    }
}

public class RegisterAccountInputDtoTests
{
    [Fact]
    public void RegisterAccountInputDto_CanSetProperties()
    {
        var dto = new RegisterAccountInputDto
        {
            Account = "testuser",
            Password = "password123",
        };

        Assert.Equal("testuser", dto.Account);
        Assert.Equal("password123", dto.Password);
    }
}

public class RoleDataDtoTests
{
    [Fact]
    public void RoleDataDto_CanSetProperties()
    {
        var dto = new RoleDataDto
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "Administrator role",
            IsActive = true
        };

        Assert.Equal("Admin", dto.Name);
        Assert.Equal("Administrator role", dto.Description);
        Assert.True(dto.IsActive);
    }
}

public class PermissionDataDtoTests
{
    [Fact]
    public void PermissionDataDto_CanSetProperties()
    {
        var dto = new PermissionDataDto
        {
            Id = Guid.NewGuid(),
            Name = "user.create",
            Description = "Create users",
            IsActive = true
        };

        Assert.Equal("user.create", dto.Name);
        Assert.Equal("Create users", dto.Description);
        Assert.True(dto.IsActive);
    }
}

public class PageResultTests
{
    [Fact]
    public void PageResult_CanSetProperties()
    {
        var items = new List<string> { "item1", "item2" };
        var result = new PageResult<string>
        {
            Items = items,
            Total = 100
        };

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(100, result.Total);
    }

    [Fact]
    public void PageResult_DefaultItemsIsEmptyList()
    {
        var result = new PageResult<string>();

        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }
}
