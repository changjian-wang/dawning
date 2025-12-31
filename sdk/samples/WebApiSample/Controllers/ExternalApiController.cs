using Dawning.Core.Results;
using Dawning.Extensions;
using Dawning.Resilience.Policies;
using Microsoft.AspNetCore.Mvc;
using WebApiSample.Services;

namespace WebApiSample.Controllers;

/// <summary>
/// External API controller - Demonstrates resilience policies
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExternalApiController : ControllerBase
{
    private readonly IExternalApiClient _apiClient;
    private readonly ResiliencePolicyBuilder _policyBuilder;
    private readonly ILogger<ExternalApiController> _logger;

    public ExternalApiController(
        IExternalApiClient apiClient,
        ResiliencePolicyBuilder policyBuilder,
        ILogger<ExternalApiController> logger
    )
    {
        _apiClient = apiClient;
        _policyBuilder = policyBuilder;
        _logger = logger;
    }

    /// <summary>
    /// Get posts list
    /// Demo: Resilient HttpClient (automatic retry, circuit breaker)
    /// </summary>
    [HttpGet("posts")]
    public async Task<ActionResult<ApiResult<IEnumerable<PostDto>>>> GetPosts()
    {
        var posts = await _apiClient.GetPostsAsync();

        // Using CollectionExtensions
        var result = posts.OrEmpty().Take(10).ToList();

        return Ok(ApiResults.Ok<IEnumerable<PostDto>>(result));
    }

    /// <summary>
    /// Get single post
    /// Demo: Manual resilience policy usage
    /// </summary>
    [HttpGet("posts/{id}")]
    public async Task<ActionResult<ApiResult<PostDto>>> GetPost(int id)
    {
        // Manually build resilience pipeline
        var pipeline = _policyBuilder.Build<PostDto?>();

        var post = await pipeline.ExecuteAsync(
            async ct =>
            {
                _logger.LogInformation("Getting post {PostId}", id);
                return await _apiClient.GetPostAsync(id);
            },
            HttpContext.RequestAborted
        );

        if (post == null)
        {
            return Ok(ApiResults.Fail<PostDto>("NOT_FOUND", "Post not found"));
        }

        return Ok(ApiResults.Ok(post));
    }

    /// <summary>
    /// Batch get posts
    /// Demo: CollectionExtensions.Batch for batch processing
    /// </summary>
    [HttpPost("posts/batch")]
    public async Task<ActionResult<ApiResult<IEnumerable<PostDto>>>> GetPostsBatch(
        [FromBody] int[] ids
    )
    {
        var results = new List<PostDto>();

        // Use Batch for batch processing to avoid too many requests at once
        foreach (var batch in ids.Batch(5))
        {
            var batchIds = batch.ToList();
            _logger.LogInformation("Processing batch: {Ids}", batchIds.JoinToString(", "));

            foreach (var id in batchIds)
            {
                var post = await _apiClient.GetPostAsync(id);
                if (post != null)
                {
                    results.Add(post);
                }
            }
        }

        return Ok(ApiResults.Ok<IEnumerable<PostDto>>(results));
    }
}

public class PostDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Use StringExtensions.Truncate to truncate content
    /// </summary>
    public string Summary => Body.Truncate(100);
}
