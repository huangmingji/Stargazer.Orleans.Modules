using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Integration;

public class TemplateControllerIntegrationTests : IntegrationTestBase
{
    public TemplateControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var input = new CreateTemplateInputDto
        {
            Name = "",
            Code = $"test_code_{Guid.NewGuid():N}",
            Channel = MessageChannel.Email,
            ContentTemplate = "Test content {{name}}"
        };

        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input);

        Assert.False(success);
        Assert.Equal("invalid_name", errorCode);
    }

    [Fact]
    public async Task Create_WithEmptyCode_ReturnsBadRequest()
    {
        var input = new CreateTemplateInputDto
        {
            Name = "Test Template",
            Code = "",
            Channel = MessageChannel.Email,
            ContentTemplate = "Test content"
        };

        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input);

        Assert.False(success);
        Assert.Equal("invalid_code", errorCode);
    }

    [Fact]
    public async Task Create_WithEmptyContent_ReturnsBadRequest()
    {
        var input = new CreateTemplateInputDto
        {
            Name = "Test Template",
            Code = $"test_code_{Guid.NewGuid():N}",
            Channel = MessageChannel.Email,
            ContentTemplate = ""
        };

        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input);

        Assert.False(success);
        Assert.Equal("invalid_content", errorCode);
    }

    [Fact]
    public async Task Create_WithValidInput_ReturnsSuccess()
    {
        var input = new CreateTemplateInputDto
        {
            Name = $"Test Template {Guid.NewGuid():N}",
            Code = $"test_code-{Guid.NewGuid():N}",
            Channel = MessageChannel.Email,
            SubjectTemplate = "Test Subject",
            ContentTemplate = "Hello {{name}}, your order {{orderId}} is ready.",
            Description = "Test template description"
        };

        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(input.Name, data.Name);
        Assert.Equal(input.Code, data.Code);
        Assert.Equal(input.Channel.ToString(), data.Channel);
    }

    [Fact]
    public async Task Create_WithVariables_ReturnsSuccess()
    {
        var input = new CreateTemplateInputDto
        {
            Name = $"Template With Variables {Guid.NewGuid():N}",
            Code = $"var-code-{Guid.NewGuid():N}",
            Channel = MessageChannel.Sms,
            ContentTemplate = "Dear {{name}}, your code is {{code}}",
            Variables = new List<TemplateVariableDto>
            {
                new() { Name = "name", Type = "string", Required = true },
                new() { Name = "code", Type = "string", Required = true }
            }
        };

        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.NotNull(data.Variables);
        Assert.Equal(2, data.Variables.Count);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsError()
    {
        var code = $"dup-code-{Guid.NewGuid():N}";
        
        var input1 = new CreateTemplateInputDto
        {
            Name = "First Template",
            Code = code,
            Channel = MessageChannel.Email,
            ContentTemplate = "First content"
        };
        await PostAsync<TemplateDto>("api/template", input1);

        var input2 = new CreateTemplateInputDto
        {
            Name = "Second Template",
            Code = code,
            Channel = MessageChannel.Email,
            ContentTemplate = "Second content"
        };
        var (success, data, errorCode) = await PostAsync<TemplateDto>("api/template", input2);

        Assert.False(success);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsBadRequest()
    {
        var input = new UpdateTemplateInputDto
        {
            Id = Guid.Empty,
            Name = "Updated Name",
            Code = "updated-code",
            ContentTemplate = "Updated content"
        };

        var (success, data, errorCode) = await PutAsync<TemplateDto>("api/template", input);

        Assert.False(success);
        Assert.Equal("invalid_id", errorCode);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<TemplateDto>($"api/template/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("template_not_found", errorCode);
    }

    [Fact]
    public async Task GetByCode_WithNonExistentCode_ReturnsNotFound()
    {
        var nonExistentCode = $"non-existent-{Guid.NewGuid():N}";

        var (success, data, errorCode) = await GetAsync<TemplateDto>($"api/template/code/{nonExistentCode}?channel=Email");

        Assert.False(success);
        Assert.Equal("template_not_found", errorCode);
    }

    [Fact]
    public async Task GetByChannel_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<List<TemplateDto>>($"api/template/channel/Email");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetTemplates_WithNoFilters_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedTemplateResult>($"api/template");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetTemplates_WithChannelFilter_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedTemplateResult>($"api/template?channel=Sms");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetTemplates_WithSearchText_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedTemplateResult>($"api/template?searchText=test");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetTemplates_WithIsActiveFilter_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedTemplateResult>($"api/template?isActive=true");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetTemplates_WithPagination_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedTemplateResult>($"api/template?page=1&pageSize=10");

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(1, data.Page);
        Assert.Equal(10, data.PageSize);
    }

    [Fact]
    public async Task Preview_WithNonExistentId_ReturnsError()
    {
        var nonExistentId = Guid.NewGuid();
        var variables = new Dictionary<string, string>
        {
            { "name", "Test User" }
        };

        var response = await Client.PostAsync(
            $"api/template/{nonExistentId}/preview",
            new StringContent(System.Text.Json.JsonSerializer.Serialize(variables), System.Text.Encoding.UTF8, "application/json"));
        
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                    response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsError()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"api/template/{nonExistentId}");
        
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                    response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}

public class PagedTemplateResult
{
    public List<TemplateDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
