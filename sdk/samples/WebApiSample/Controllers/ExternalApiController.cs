using Dawning.Core.Results;
using Dawning.Extensions;
using Dawning.Resilience.Policies;
using Microsoft.AspNetCore.Mvc;
using WebApiSample.Services;

namespace WebApiSample.Controllers;

/// <summary>
/// 外部 API 调用控制器 - 演示弹性策略
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
        ILogger<ExternalApiController> logger)
    {
        _apiClient = apiClient;
        _policyBuilder = policyBuilder;
        _logger = logger;
    }

    /// <summary>
    /// 获取帖子列表
    /// 演示: 弹性 HttpClient（自动重试、熔断）
    /// </summary>
    [HttpGet("posts")]
    public async Task<ActionResult<ApiResult<IEnumerable<PostDto>>>> GetPosts()
    {
        var posts = await _apiClient.GetPostsAsync();
        
        // 使用 CollectionExtensions
        var result = posts
            .OrEmpty()
            .Take(10)
            .ToList();
        
        return Ok(ApiResults.Ok<IEnumerable<PostDto>>(result));
    }

    /// <summary>
    /// 获取单个帖子
    /// 演示: 手动使用弹性策略
    /// </summary>
    [HttpGet("posts/{id}")]
    public async Task<ActionResult<ApiResult<PostDto>>> GetPost(int id)
    {
        // 手动构建弹性管道
        var pipeline = _policyBuilder.Build<PostDto?>();
        
        var post = await pipeline.ExecuteAsync(async ct =>
        {
            _logger.LogInformation("正在获取帖子 {PostId}", id);
            return await _apiClient.GetPostAsync(id);
        }, HttpContext.RequestAborted);
        
        if (post == null)
        {
            return Ok(ApiResults.Fail<PostDto>("NOT_FOUND", "帖子不存在"));
        }
        
        return Ok(ApiResults.Ok(post));
    }

    /// <summary>
    /// 批量获取帖子
    /// 演示: CollectionExtensions.Batch 分批处理
    /// </summary>
    [HttpPost("posts/batch")]
    public async Task<ActionResult<ApiResult<IEnumerable<PostDto>>>> GetPostsBatch([FromBody] int[] ids)
    {
        var results = new List<PostDto>();
        
        // 使用 Batch 分批处理，避免一次请求太多
        foreach (var batch in ids.Batch(5))
        {
            var batchIds = batch.ToList();
            _logger.LogInformation("正在处理批次: {Ids}", batchIds.JoinToString(", "));
            
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
    /// 使用 StringExtensions.Truncate 截断内容
    /// </summary>
    public string Summary => Body.Truncate(100);
}
