using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;

namespace Stargazer.Orleans.MessageManagement.Grains.Grains.Templates;

/// <summary>
/// 消息模板 Grain 实现
/// </summary>
[StatelessWorker]
public partial class TemplateGrain : Grain, ITemplateGrain
{
    private readonly IRepository<MessageTemplate, Guid> _templateRepository;
    private readonly ILogger<TemplateGrain> _logger;

    public TemplateGrain(
        IRepository<MessageTemplate, Guid> templateRepository,
        ILogger<TemplateGrain> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task<TemplateDto> CreateAsync(CreateTemplateInputDto input)
    {
        var existing = await _templateRepository.FindAsync(
            x => x.Code == input.Code && x.Channel == input.Channel);

        if (existing != null)
        {
            throw new InvalidOperationException("template_code_exists");
        }

        var template = new MessageTemplate
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Code = input.Code,
            Channel = input.Channel,
            SubjectTemplate = input.SubjectTemplate,
            ContentTemplate = input.ContentTemplate,
            Variables = input.Variables != null
                ? JsonSerializer.Serialize(input.Variables)
                : "[]",
            Description = input.Description,
            IsActive = true,
            Version = 1,
            DefaultProvider = input.DefaultProvider,
            Tags = input.Tags,
            CreatorId = Guid.Empty,
            CreationTime = DateTime.UtcNow
        };

        await _templateRepository.InsertAsync(template);

        _logger.LogInformation("Created template {TemplateId} with code {TemplateCode}",
            template.Id, template.Code);

        return ToDto(template);
    }

    public async Task<TemplateDto> UpdateAsync(UpdateTemplateInputDto input)
    {
        var template = await _templateRepository.FindAsync(input.Id);
        if (template == null)
        {
            throw new KeyNotFoundException("template_not_found");
        }

        if (template.Code != input.Code)
        {
            var existing = await _templateRepository.FindAsync(
                x => x.Code == input.Code && x.Channel == template.Channel);
            if (existing != null)
            {
                throw new InvalidOperationException("template_code_exists");
            }
        }

        template.Name = input.Name;
        template.Code = input.Code;
        template.SubjectTemplate = input.SubjectTemplate;
        template.ContentTemplate = input.ContentTemplate;
        template.Variables = input.Variables != null
            ? JsonSerializer.Serialize(input.Variables)
            : template.Variables;
        template.Description = input.Description;
        template.IsActive = input.IsActive;
        template.Version++;
        template.DefaultProvider = input.DefaultProvider;
        template.Tags = input.Tags;
        template.LastModifierId = Guid.Empty;
        template.LastModifyTime = DateTime.UtcNow;

        await _templateRepository.UpdateAsync(template);

        return ToDto(template);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _templateRepository.DeleteAsync(id);
        _logger.LogInformation("Deleted template {TemplateId}", id);
    }

    public async Task<TemplateDto?> GetAsync(Guid id)
    {
        var template = await _templateRepository.FindAsync(id);
        return template != null ? ToDto(template) : null;
    }

    public async Task<TemplateDto?> GetByCodeAsync(string code, MessageChannel channel)
    {
        var template = await _templateRepository.FindAsync(
            x => x.Code == code && x.Channel == channel && x.IsActive);
        return template != null ? ToDto(template) : null;
    }

    public async Task<List<TemplateDto>> GetByChannelAsync(MessageChannel channel)
    {
        var templates = await _templateRepository.FindListAsync(
            x => x.Channel == channel && x.IsActive);
        return templates.Select(ToDto).ToList();
    }

    public async Task<string> PreviewAsync(Guid id, Dictionary<string, string>? variables)
    {
        var template = await _templateRepository.FindAsync(id);
        if (template == null)
        {
            throw new KeyNotFoundException("template_not_found");
        }

        if (variables == null || variables.Count == 0)
        {
            return template.ContentTemplate;
        }

        return VariableRegex().Replace(template.ContentTemplate, match =>
        {
            var key = match.Groups["key"].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public async Task<PageResult<TemplateDto>> GetTemplatesAsync(MessageChannel? channel = null,
        string? searchText = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        var result = await _templateRepository.FindListAsync(
            x => (channel == null || x.Channel == channel) &&
                 (isActive == null || x.IsActive == isActive) &&
                 (string.IsNullOrEmpty(searchText) ||
                   x.Name.Contains(searchText) || x.Code.Contains(searchText)),
            pageIndex: page,
            pageSize: pageSize,
            orderByDescending: true);

        return new PageResult<TemplateDto>
        {
            Total = result.Total,
            Items = result.Items.Select(ToDto).ToList()
        };
    }

    private TemplateDto ToDto(MessageTemplate template)
    {
        List<TemplateVariableDto>? variables = null;
        if (!string.IsNullOrEmpty(template.Variables))
        {
            try
            {
                variables = JsonSerializer.Deserialize<List<TemplateVariableDto>>(template.Variables);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize variables for template {TemplateId}", template.Id);
            }
        }

        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Code = template.Code,
            Channel = template.Channel.ToString(),
            SubjectTemplate = template.SubjectTemplate,
            ContentTemplate = template.ContentTemplate,
            Variables = variables,
            Description = template.Description,
            IsActive = template.IsActive,
            Version = template.Version,
            DefaultProvider = template.DefaultProvider,
            Tags = template.Tags,
            CreationTime = template.CreationTime,
            LastModifyTime = template.LastModifyTime
        };
    }

    [GeneratedRegex(@"\{\{(?<key>\w+)\}\}")]
    private static partial Regex VariableRegex();
}
