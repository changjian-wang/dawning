using Dawning.Extensions;
using WebApiSample.Controllers;

namespace WebApiSample.Services;

/// <summary>
/// 外部 API 客户端接口
/// </summary>
public interface IExternalApiClient
{
    Task<IEnumerable<PostDto>> GetPostsAsync();
    Task<PostDto?> GetPostAsync(int id);
}

/// <summary>
/// 外部 API 客户端实现
/// 使用弹性 HttpClient（自动重试、熔断）
/// </summary>
public class ExternalApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiClient> _logger;

    public ExternalApiClient(HttpClient httpClient, ILogger<ExternalApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<PostDto>> GetPostsAsync()
    {
        _logger.LogInformation("正在获取帖子列表");
        
        var response = await _httpClient.GetStringAsync("/posts");
        var posts = response.FromJson<List<PostDto>>();
        
        return posts.OrEmpty();
    }

    public async Task<PostDto?> GetPostAsync(int id)
    {
        _logger.LogInformation("正在获取帖子 {PostId}", id);
        
        try
        {
            var response = await _httpClient.GetStringAsync($"/posts/{id}");
            return response.FromJson<PostDto>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("帖子 {PostId} 不存在", id);
            return null;
        }
    }
}
