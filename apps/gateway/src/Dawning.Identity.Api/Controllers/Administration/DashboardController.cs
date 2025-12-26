using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Application.Interfaces.MultiTenancy;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// Dashboard controller - provides real-time statistics
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/dashboard")]
    [Route("api/v{version:apiVersion}/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IAuditLogService _auditLogService;
        private readonly IApplicationService _applicationService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IUserService userService,
            IRoleService roleService,
            IAuditLogService auditLogService,
            IApplicationService applicationService,
            ITenantService tenantService,
            ILogger<DashboardController> logger
        )
        {
            _userService = userService;
            _roleService = roleService;
            _auditLogService = auditLogService;
            _applicationService = applicationService;
            _tenantService = tenantService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics overview
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                // Get user count
                var userResult = await _userService.GetPagedListAsync(new UserModel(), 1, 1);
                var totalUsers = userResult.TotalCount;

                // Get role count
                var roles = await _roleService.GetAllAsync();
                var totalRoles = roles.Count();

                // Get today's audit logs count
                var todayModel = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow
                };
                var todayLogs = await _auditLogService.GetPagedListAsync(todayModel, 1, 1);
                var todayLogsCount = todayLogs.TotalCount;

                // Get yesterday's audit logs count for comparison
                var yesterdayModel = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.Date.AddDays(-1),
                    EndDate = DateTime.UtcNow.Date
                };
                var yesterdayLogs = await _auditLogService.GetPagedListAsync(yesterdayModel, 1, 1);
                var yesterdayLogsCount = yesterdayLogs.TotalCount;

                // Calculate growth rate
                var growthRate = yesterdayLogsCount > 0
                    ? Math.Round(
                          ((double)(todayLogsCount - yesterdayLogsCount) / yesterdayLogsCount)
                              * 100,
                          1
                      )
                    : 0;

                // Get application count
                var apps = await _applicationService.GetAllAsync();
                var totalApps = apps.Count();

                var stats = new DashboardStatsDto
                {
                    TotalUsers = totalUsers,
                    TotalRoles = totalRoles,
                    TodayAuditLogs = todayLogsCount,
                    TotalApplications = totalApps,
                    GrowthRate = growthRate
                };

                return Ok(new { code = 20000, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return Ok(
                    new
                    {
                        code = 20000,
                        data = new DashboardStatsDto()
                    }
                );
            }
        }

        /// <summary>
        /// Get audit log trend data for chart
        /// </summary>
        [HttpGet("content-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContentData()
        {
            try
            {
                var chartData = new List<ChartDataPoint>();

                // Get data for last 8 days
                for (int i = 7; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var model = new AuditLogModel
                    {
                        StartDate = date,
                        EndDate = date.AddDays(1)
                    };
                    var result = await _auditLogService.GetPagedListAsync(model, 1, 1);

                    chartData.Add(
                        new ChartDataPoint { X = date.ToString("yyyy-MM-dd"), Y = result.TotalCount }
                    );
                }

                return Ok(new { code = 20000, data = chartData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content data");
                return Ok(new { code = 20000, data = new List<ChartDataPoint>() });
            }
        }

        /// <summary>
        /// Get recent activities (audit logs)
        /// </summary>
        [HttpGet("recent-activities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentActivities([FromQuery] string? type = null)
        {
            try
            {
                var model = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow,
                    EntityType = type
                };
                var result = await _auditLogService.GetPagedListAsync(model, 1, 10);

                var activities = result
                    .Items.Select(
                        (log, index) =>
                            new ActivityItem
                            {
                                Key = index + 1,
                                Title = $"{log.Action} {log.EntityType}",
                                Description = log.Description ?? "",
                                Time = log.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                                User = log.UserId.ToString()
                            }
                    )
                    .ToList();

                return Ok(new { code = 20000, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities");
                return Ok(new { code = 20000, data = new List<ActivityItem>() });
            }
        }

        /// <summary>
        /// Get entity type distribution for pie chart
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                // Get all audit logs from last 30 days
                var model = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow
                };
                var result = await _auditLogService.GetPagedListAsync(model, 1, 1000);

                // Group by entity type
                var groups = result
                    .Items.GroupBy(x => x.EntityType ?? "Other")
                    .Select(g => new CategoryItem { Name = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .ToList();

                var total = groups.Sum(x => x.Value);

                return Ok(new { code = 20000, data = new { categories = groups, total } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Ok(
                    new
                    {
                        code = 20000,
                        data = new { categories = new List<CategoryItem>(), total = 0 }
                    }
                );
            }
        }

        /// <summary>
        /// Get announcements (system notifications)
        /// </summary>
        [HttpGet("announcements")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAnnouncements()
        {
            try
            {
                // Get recent important audit logs as announcements
                var model = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow
                };
                var result = await _auditLogService.GetPagedListAsync(model, 1, 5);

                var announcements = result
                    .Items.Select(log => new AnnouncementItem
                    {
                        Type = GetAnnouncementType(log.Action),
                        Label = GetAnnouncementLabel(log.Action),
                        Content = log.Description ?? $"{log.Action} {log.EntityType}"
                    })
                    .ToList();

                return Ok(new { code = 20000, data = announcements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving announcements");
                return Ok(new { code = 20000, data = new List<AnnouncementItem>() });
            }
        }

        private static string GetAnnouncementType(string action)
        {
            return action switch
            {
                "Create" => "green",
                "Update" => "blue",
                "Delete" => "orangered",
                "Login" => "cyan",
                "Logout" => "cyan",
                _ => "blue"
            };
        }

        private static string GetAnnouncementLabel(string action)
        {
            return action switch
            {
                "Create" => "Created",
                "Update" => "Updated",
                "Delete" => "Deleted",
                "Login" => "Login",
                "Logout" => "Logout",
                _ => "Activity"
            };
        }
    }

    public class DashboardStatsDto
    {
        public long TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public long TodayAuditLogs { get; set; }
        public int TotalApplications { get; set; }
        public double GrowthRate { get; set; }
    }

    public class ChartDataPoint
    {
        public string X { get; set; } = "";
        public long Y { get; set; }
    }

    public class ActivityItem
    {
        public int Key { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Time { get; set; } = "";
        public string User { get; set; } = "";
    }

    public class CategoryItem
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    public class AnnouncementItem
    {
        public string Type { get; set; } = "";
        public string Label { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
