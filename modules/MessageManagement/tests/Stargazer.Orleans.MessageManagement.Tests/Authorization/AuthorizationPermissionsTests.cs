using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Authorization;

public class AuthorizationPermissionsTests
{
    [Fact]
    public void Messages_Send_ShouldHaveCorrectValue()
    {
        Assert.Equal("message.send", AuthorizationPermissions.Messages.Send);
    }

    [Fact]
    public void Messages_View_ShouldHaveCorrectValue()
    {
        Assert.Equal("message.view", AuthorizationPermissions.Messages.View);
    }

    [Fact]
    public void Messages_Retry_ShouldHaveCorrectValue()
    {
        Assert.Equal("message.retry", AuthorizationPermissions.Messages.Retry);
    }

    [Fact]
    public void Messages_Cancel_ShouldHaveCorrectValue()
    {
        Assert.Equal("message.cancel", AuthorizationPermissions.Messages.Cancel);
    }

    [Fact]
    public void Templates_View_ShouldHaveCorrectValue()
    {
        Assert.Equal("template.view", AuthorizationPermissions.Templates.View);
    }

    [Fact]
    public void Templates_Create_ShouldHaveCorrectValue()
    {
        Assert.Equal("template.create", AuthorizationPermissions.Templates.Create);
    }

    [Fact]
    public void Templates_Update_ShouldHaveCorrectValue()
    {
        Assert.Equal("template.update", AuthorizationPermissions.Templates.Update);
    }

    [Fact]
    public void Templates_Delete_ShouldHaveCorrectValue()
    {
        Assert.Equal("template.delete", AuthorizationPermissions.Templates.Delete);
    }

    [Fact]
    public void Messages_PermissionCodes_ShouldFollowNamingConvention()
    {
        Assert.StartsWith("message.", AuthorizationPermissions.Messages.Send);
        Assert.StartsWith("message.", AuthorizationPermissions.Messages.View);
        Assert.StartsWith("message.", AuthorizationPermissions.Messages.Retry);
        Assert.StartsWith("message.", AuthorizationPermissions.Messages.Cancel);
    }

    [Fact]
    public void Templates_PermissionCodes_ShouldFollowNamingConvention()
    {
        Assert.StartsWith("template.", AuthorizationPermissions.Templates.View);
        Assert.StartsWith("template.", AuthorizationPermissions.Templates.Create);
        Assert.StartsWith("template.", AuthorizationPermissions.Templates.Update);
        Assert.StartsWith("template.", AuthorizationPermissions.Templates.Delete);
    }

    [Fact]
    public void AllPermissions_ShouldBeUnique()
    {
        var allPermissions = new[]
        {
            AuthorizationPermissions.Messages.Send,
            AuthorizationPermissions.Messages.View,
            AuthorizationPermissions.Messages.Retry,
            AuthorizationPermissions.Messages.Cancel,
            AuthorizationPermissions.Templates.View,
            AuthorizationPermissions.Templates.Create,
            AuthorizationPermissions.Templates.Update,
            AuthorizationPermissions.Templates.Delete
        };

        var uniquePermissions = allPermissions.Distinct().ToList();
        Assert.Equal(allPermissions.Length, uniquePermissions.Count);
    }

    [Theory]
    [InlineData(AuthorizationPermissions.Messages.Send, "permission:message.send")]
    [InlineData(AuthorizationPermissions.Messages.View, "permission:message.view")]
    [InlineData(AuthorizationPermissions.Templates.Create, "permission:template.create")]
    public void PolicyName_ShouldBeCorrectlyFormatted(string permission, string expectedPolicy)
    {
        var policyName = $"permission:{permission}";
        Assert.Equal(expectedPolicy, policyName);
    }
}
