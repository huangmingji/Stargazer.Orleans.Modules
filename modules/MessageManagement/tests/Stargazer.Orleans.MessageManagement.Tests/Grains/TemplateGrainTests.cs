using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Grains;

public class TemplateGrainTests
{
    [Fact]
    public void CreateTemplateInputDto_ShouldSetProperties()
    {
        var input = new CreateTemplateInputDto
        {
            Name = "Welcome Email",
            Code = "welcome_email",
            Channel = MessageChannel.Email,
            SubjectTemplate = "Welcome {{name}}!",
            ContentTemplate = "Hello {{name}}, welcome to our service!"
        };

        Assert.Equal("Welcome Email", input.Name);
        Assert.Equal("welcome_email", input.Code);
        Assert.Equal(MessageChannel.Email, input.Channel);
        Assert.Equal("Welcome {{name}}!", input.SubjectTemplate);
    }

    [Fact]
    public void UpdateTemplateInputDto_ShouldSetProperties()
    {
        var input = new UpdateTemplateInputDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Name",
            Code = "updated_code",
            ContentTemplate = "Updated content",
            IsActive = true
        };

        Assert.NotEqual(Guid.Empty, input.Id);
        Assert.Equal("Updated Name", input.Name);
        Assert.Equal("updated_code", input.Code);
        Assert.True(input.IsActive);
    }

    [Fact]
    public void TemplateDto_ShouldSetProperties()
    {
        var dto = new TemplateDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Code = "test_code",
            Channel = MessageChannel.Email.ToString(),
            SubjectTemplate = "Subject",
            ContentTemplate = "Content",
            IsActive = true,
            Version = 1
        };

        Assert.Equal("Test Template", dto.Name);
        Assert.Equal("test_code", dto.Code);
        Assert.True(dto.IsActive);
        Assert.Equal(1, dto.Version);
    }

    [Fact]
    public void TemplateVariableDto_ShouldSetProperties()
    {
        var variable = new TemplateVariableDto
        {
            Name = "username",
            Type = "string",
            Required = true,
            DefaultValue = "Guest"
        };

        Assert.Equal("username", variable.Name);
        Assert.Equal("string", variable.Type);
        Assert.True(variable.Required);
        Assert.Equal("Guest", variable.DefaultValue);
    }

    [Fact]
    public void MessageTemplate_ShouldInitializeCorrectly()
    {
        var template = new MessageTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Template",
            Code = "test_code",
            Channel = MessageChannel.Email,
            ContentTemplate = "Hello {{name}}!"
        };

        Assert.True(template.IsActive);
        Assert.Equal(1, template.Version);
    }

    [Fact]
    public void MessageTemplate_WithVariables_ShouldSupportVariableReplacement()
    {
        var template = new MessageTemplate
        {
            ContentTemplate = "Hello {{name}}, your code is {{code}}"
        };

        var content = template.ContentTemplate;
        content = content.Replace("{{name}}", "John");
        content = content.Replace("{{code}}", "12345");

        Assert.Equal("Hello John, your code is 12345", content);
    }

    [Theory]
    [InlineData(MessageChannel.Email)]
    [InlineData(MessageChannel.Sms)]
    [InlineData(MessageChannel.Push)]
    public void CreateTemplateInputDto_ShouldSupportAllChannels(MessageChannel channel)
    {
        var input = new CreateTemplateInputDto
        {
            Name = "Test",
            Code = "test",
            Channel = channel
        };

        Assert.Equal(channel, input.Channel);
    }

    [Theory]
    [InlineData(MessageChannel.Email, 1)]
    [InlineData(MessageChannel.Sms, 2)]
    [InlineData(MessageChannel.Push, 3)]
    public void MessageChannel_ShouldHaveCorrectValues(MessageChannel channel, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)channel);
    }
}
