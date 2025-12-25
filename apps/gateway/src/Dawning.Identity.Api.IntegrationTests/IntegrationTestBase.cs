using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dawning.Identity.Api.IntegrationTests;

/// <summary>
/// 集成测试基类，提供公共功能
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    /// <summary>
    /// 设置 Bearer Token
    /// </summary>
    protected void SetBearerToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// 清除 Authorization Header
    /// </summary>
    protected void ClearBearerToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// POST 请求并返回反序列化的响应
    /// </summary>
    protected async Task<T?> PostAsync<T>(string url, object content)
    {
        var response = await Client.PostAsJsonAsync(url, content, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// GET 请求并返回反序列化的响应
    /// </summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// PUT 请求并返回反序列化的响应
    /// </summary>
    protected async Task<T?> PutAsync<T>(string url, object content)
    {
        var response = await Client.PutAsJsonAsync(url, content, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// DELETE 请求
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
