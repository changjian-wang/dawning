using Dawning.Core.Exceptions;
using Dawning.Core.Results;
using Dawning.Extensions;
using Dawning.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Controllers;

/// <summary>
/// 用户管理控制器 - 演示 Dawning SDK 使用
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly CurrentUser _currentUser;
    private readonly ILogger<UsersController> _logger;
    
    // 模拟数据库
    private static readonly List<UserDto> _users = new()
    {
        new UserDto { Id = 1, Name = "张三", Email = "zhangsan@example.com", Phone = "13812345678", CreatedAt = DateTime.Now.AddDays(-30) },
        new UserDto { Id = 2, Name = "李四", Email = "lisi@example.com", Phone = "13987654321", CreatedAt = DateTime.Now.AddDays(-20) },
        new UserDto { Id = 3, Name = "王五", Email = "wangwu@example.com", Phone = "13611112222", CreatedAt = DateTime.Now.AddDays(-10) }
    };

    public UsersController(CurrentUser currentUser, ILogger<UsersController> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表（分页）
    /// 演示: PagedResult, CollectionExtensions
    /// </summary>
    [HttpGet]
    public ActionResult<PagedResult<UserDto>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = _users.AsQueryable();
        
        // 使用 StringExtensions 检查搜索条件
        if (!search.IsNullOrWhiteSpace())
        {
            query = query.Where(u => 
                u.Name.Contains(search!) || 
                u.Email.Contains(search!));
        }
        
        var total = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        // 使用 PagedResult 返回分页数据
        return Ok(new PagedResult<UserDto>(items, total, page, pageSize));
    }

    /// <summary>
    /// 获取单个用户
    /// 演示: ApiResult, NotFoundException
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<ApiResult<UserDto>> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        
        if (user == null)
        {
            // 抛出业务异常，由异常中间件统一处理
            throw new NotFoundException($"用户 {id} 不存在");
        }
        
        // 使用 ApiResult 返回成功响应
        return Ok(ApiResults.Ok(user));
    }

    /// <summary>
    /// 创建用户
    /// 演示: ValidationException, StringExtensions
    /// </summary>
    [HttpPost]
    public ActionResult<ApiResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // 使用 StringExtensions 验证
        if (request.Name.IsNullOrWhiteSpace())
        {
            throw new ValidationException("用户名不能为空");
        }
        
        if (!request.Email.IsValidEmail())
        {
            throw new ValidationException("邮箱格式不正确");
        }
        
        if (!request.Phone.IsValidPhoneNumber())
        {
            throw new ValidationException("手机号格式不正确");
        }
        
        // 检查邮箱是否已存在
        if (_users.Any(u => u.Email == request.Email))
        {
            throw new BusinessException("邮箱已被使用");
        }
        
        var user = new UserDto
        {
            Id = _users.Max(u => u.Id) + 1,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone.Mask(), // 使用 Mask 隐藏手机号
            CreatedAt = DateTime.Now
        };
        
        _users.Add(user);
        
        _logger.LogInformation("用户 {UserId} 创建成功", user.Id);
        
        return Ok(ApiResults.Ok(user, "创建成功"));
    }

    /// <summary>
    /// 获取当前登录用户信息
    /// 演示: CurrentUser, Authorize
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResult<object>> GetCurrentUser()
    {
        // 使用 CurrentUser 获取当前用户信息
        var userInfo = new
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            Roles = _currentUser.Roles,
            TenantId = _currentUser.TenantId
        };
        
        return Ok(ApiResults.Ok(userInfo));
    }
}

// ========================================
// DTOs
// ========================================

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 使用 DateTimeExtensions 获取相对时间
    /// </summary>
    public string CreatedAgo => CreatedAt.ToRelativeTime();
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
