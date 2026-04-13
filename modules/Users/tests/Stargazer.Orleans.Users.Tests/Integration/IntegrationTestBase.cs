using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Stargazer.Common.Extend;
using Xunit;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Stargazer.Orleans.Users.Tests.Integration;

public class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private string? _accessToken;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.HttpClient;
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected void SetAuthToken(string accessToken)
    {
        _accessToken = accessToken;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    protected void ClearAuthToken()
    {
        _accessToken = null;
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<(bool Success, T? Data, string? ErrorCode)> PostAsync<T>(
        string url, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
        ApplyAuthHeader(request);

        var response = await Client.SendAsync(request);
        return await ParseResponseAsync<T>(response);
    }

    protected async Task<(bool Success, T? Data, string? ErrorCode)> GetAsync<T>(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuthHeader(request);

        var response = await Client.SendAsync(request);
        return await ParseResponseAsync<T>(response);
    }

    protected async Task<(bool Success, T? Data, string? ErrorCode)> PutAsync<T>(
        string url, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
        ApplyAuthHeader(request);

        var response = await Client.SendAsync(request);
        return await ParseResponseAsync<T>(response);
    }
    
    protected async Task<(bool Success, T? Data, string? ErrorCode)> PatchAsync<T>(
        string url, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url);
        if (body != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        }
        ApplyAuthHeader(request);

        var response = await Client.SendAsync(request);
        return await ParseResponseAsync<T>(response);
    }

    protected async Task<(bool Success, T? Data, string? ErrorCode)> DeleteAsync<T>(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        ApplyAuthHeader(request);

        var response = await Client.SendAsync(request);
        return await ParseResponseAsync<T>(response);
    }

    private void ApplyAuthHeader(HttpRequestMessage request)
    {
        if (_accessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }
    }

    private async Task<(bool Success, T? Data, string? ErrorCode)> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, default, "unauthorized");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return (false, default, "forbidden");
        }
        
        if (string.IsNullOrWhiteSpace(content))
        {
            return (false, default, "empty_response");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            string? errorCode = null;
            if (root.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
            {
                errorCode = codeElement.GetString();
            }
            return (false, default, errorCode);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            return (false, default, $"http_error: {response.StatusCode}");
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return (true, default, null);
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return (true, default, null);
            }

            var success = response.StatusCode == HttpStatusCode.OK;
            string? errorCode = null;
            if (root.TryGetProperty("code", out var codeElement) && codeElement.ValueKind == JsonValueKind.String)
            {
                errorCode = codeElement.GetString();
            }

            T? data = default;
            if (success)
            {
                root.TryGetProperty("data", out var dataElement);
                if (dataElement.ValueKind == JsonValueKind.Object || dataElement.ValueKind == JsonValueKind.Array)
                {
                    var dataText = dataElement.GetRawText();
                    data = JsonConvert.DeserializeObject<T>(dataText);
                }
            }

            return (success, data, errorCode);
        }
        catch (JsonException ex)
        {
            return (false, default, $"json_error: {ex.Message}");
        }
    }
}
