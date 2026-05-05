using System.Collections.Concurrent;
using System.Text.Json;

namespace Stargazer.Orleans.WechatManagement.Silo.Resources;

public class LocalizationService
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources = new();
    private readonly string _resourcesPath;

    public LocalizationService(IHostEnvironment env)
    {
        _resourcesPath = Path.Combine(env.ContentRootPath, "Resources");
        LoadResources();
    }

    private void LoadResources()
    {
        if (!Directory.Exists(_resourcesPath)) return;

        var files = Directory.GetFiles(_resourcesPath, "Strings.*.json");
        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var lang = fileName["Strings.".Length..];
            var content = File.ReadAllText(file);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();
            _resources[lang] = dict;
        }
    }

    public string GetString(string code, string language = "en")
    {
        if (_resources.TryGetValue(language, out var dict) && dict.TryGetValue(code, out var value))
        {
            return value;
        }

        if (_resources.TryGetValue("en", out var enDict) && enDict.TryGetValue(code, out var enValue))
        {
            return enValue;
        }

        return code;
    }

    public string GetCurrentLanguage(HttpContext context)
    {
        var lang = context.Request.Query["lang"].ToString();
        if (!string.IsNullOrEmpty(lang)) return lang;

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            var first = acceptLanguage.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(first)) return first;
        }

        return "en";
    }
}
