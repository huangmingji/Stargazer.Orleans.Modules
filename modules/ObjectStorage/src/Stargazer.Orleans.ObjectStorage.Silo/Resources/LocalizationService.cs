using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Stargazer.Orleans.ObjectStorage.Silo.Resources;

public class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _resources = new();
    private const string DefaultLanguage = "zh-CN";
    private const string QueryParam = "lang";
    private readonly ILogger<LocalizationService> _logger;

    public LocalizationService(IWebHostEnvironment env, ILogger<LocalizationService> logger)
    {
        _logger = logger;
        LoadResources(env.ContentRootPath);
    }

    private void LoadResources(string basePath)
    {
        var files = new[] { "Strings.zh-CN.json", "Strings.en.json" };
        foreach (var file in files)
        {
            var path = Path.Combine(basePath, "Resources", file);
            if (!File.Exists(path)) continue;

            var json = File.ReadAllText(path);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null) continue;

            var lang = file.Replace("Strings.", "").Replace(".json", "");
            _resources[lang] = dict;
        }
    }

    public string GetCurrentLanguage(HttpContext context)
    {
        var query = context.Request.Query[QueryParam].FirstOrDefault();
        if (!string.IsNullOrEmpty(query))
        {
            return _resources.ContainsKey(query) ? query : DefaultLanguage;
        }

        var header = context.Request.Headers.AcceptLanguage.FirstOrDefault();
        if (!string.IsNullOrEmpty(header))
        {
            var lang = header.Split(',')[0].Trim();
            if (_resources.ContainsKey(lang))
            {
                return lang;
            }
            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                return "en";
            }
            return DefaultLanguage;
        }

        return DefaultLanguage;
    }

    public string GetMessage(string code, string language)
    {
        if (_resources.TryGetValue(language, out var dict) && dict.TryGetValue(code, out var message))
        {
            return message;
        }

        if (_resources.TryGetValue(DefaultLanguage, out var defaultDict) && defaultDict.TryGetValue(code, out var defaultMessage))
        {
            return defaultMessage;
        }

        _logger.LogWarning("Missing translation key: {Code} for language: {Language}", code, language);
        return code;
    }
}
