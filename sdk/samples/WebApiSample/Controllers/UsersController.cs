using Dawning.Core.Exceptions;
using Dawning.Core.Results;
using Dawning.Extensions;
using Dawning.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Controllers;

/// <summary>
/// User management controller - Demonstrates Dawning SDK usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly CurrentUser _currentUser;
    private readonly ILogger<UsersController> _logger;

    // Mock database
    private static readonly List<UserDto> _users = new()
    {
        new UserDto
        {
            Id = 1,
            Name = "John Doe",
            Email = "zhangsan@example.com",
            Phone = "13812345678",
            CreatedAt = DateTime.Now.AddDays(-30),
        },
        new UserDto
        {
            Id = 2,
            Name = "Jane Smith",
            Email = "lisi@example.com",
            Phone = "13987654321",
            CreatedAt = DateTime.Now.AddDays(-20),
        },
        new UserDto
        {
            Id = 3,
            Name = "Bob Wilson",
            Email = "wangwu@example.com",
            Phone = "13611112222",
            CreatedAt = DateTime.Now.AddDays(-10),
        },
    };

    public UsersController(CurrentUser currentUser, ILogger<UsersController> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Get user list (paginated)
    /// Demo: PagedResult, CollectionExtensions
    /// </summary>
    [HttpGet]
    public ActionResult<PagedResult<UserDto>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null
    )
    {
        var query = _users.AsQueryable();

        // Use StringExtensions to check search conditions
        if (!search.IsNullOrWhiteSpace())
        {
            query = query.Where(u => u.Name.Contains(search!) || u.Email.Contains(search!));
        }

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Use PagedResult to return paginated data
        return Ok(new PagedResult<UserDto>(items, total, page, pageSize));
    }

    /// <summary>
    /// Get single user
    /// Demo: ApiResult, NotFoundException
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<ApiResult<UserDto>> GetUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            // Throw business exception, handled uniformly by exception middleware
            throw new NotFoundException($"User {id} not found");
        }

        // Use ApiResult to return success response
        return Ok(ApiResults.Ok(user));
    }

    /// <summary>
    /// Create user
    /// Demo: ValidationException, StringExtensions
    /// </summary>
    [HttpPost]
    public ActionResult<ApiResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // Use StringExtensions for validation
        if (request.Name.IsNullOrWhiteSpace())
        {
            throw new ValidationException("Username cannot be empty");
        }

        if (!request.Email.IsValidEmail())
        {
            throw new ValidationException("Invalid email format");
        }

        if (!request.Phone.IsValidPhoneNumber())
        {
            throw new ValidationException("Invalid phone number format");
        }

        // Check if email already exists
        if (_users.Any(u => u.Email == request.Email))
        {
            throw new BusinessException("Email already in use");
        }

        var user = new UserDto
        {
            Id = _users.Max(u => u.Id) + 1,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone.Mask(), // Use Mask to hide phone number
            CreatedAt = DateTime.Now,
        };

        _users.Add(user);

        _logger.LogInformation("User {UserId} created successfully", user.Id);

        return Ok(ApiResults.Ok(user, "Created successfully"));
    }

    /// <summary>
    /// Get current logged-in user info
    /// Demo: CurrentUser, Authorize
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResult<object>> GetCurrentUser()
    {
        // Use CurrentUser to get current user info
        var userInfo = new
        {
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Email = _currentUser.Email,
            Roles = _currentUser.Roles,
            TenantId = _currentUser.TenantId,
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
    /// Use DateTimeExtensions to get relative time
    /// </summary>
    public string CreatedAgo => CreatedAt.ToRelativeTime();
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
