using Dawning.Extensions;
using WebApiSample.Controllers;

namespace WebApiSample.Services;

/// <summary>
/// External API client interface
/// </summary>
public interface IExternalApiClient
{
    Task<IEnumerable<PostDto>> GetPostsAsync();
    Task<PostDto?> GetPostAsync(int id);
}

/// <summary>
/// External API client implementation
/// Uses resilient HttpClient (automatic retry, circuit breaker)
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
        _logger.LogInformation("Getting posts list");

        var response = await _httpClient.GetStringAsync("/posts");
        var posts = response.FromJson<List<PostDto>>();

        return posts.OrEmpty();
    }

    public async Task<PostDto?> GetPostAsync(int id)
    {
        _logger.LogInformation("Getting post {PostId}", id);

        try
        {
            var response = await _httpClient.GetStringAsync($"/posts/{id}");
            return response.FromJson<PostDto>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Post {PostId} not found", id);
            return null;
        }
    }
}
