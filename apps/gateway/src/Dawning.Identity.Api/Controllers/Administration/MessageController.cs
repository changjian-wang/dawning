using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Interfaces.Administration;
using Dawning.Identity.Domain.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dawning.Identity.Api.Controllers.Administration
{
    /// <summary>
    /// Message notification controller
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/message")]
    [Route("api/v{version:apiVersion}/message")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(
            IAuditLogService auditLogService,
            ILogger<MessageController> logger
        )
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get message list (recent audit logs as notifications)
        /// </summary>
        [HttpPost("list")]
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList()
        {
            try
            {
                // Get recent audit logs as messages
                var model = new AuditLogModel
                {
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow,
                };
                var result = await _auditLogService.GetPagedListAsync(model, 1, 20);

                // Transform audit logs to message format
                var messages = result
                    .Items.Select(
                        (log, index) =>
                            new MessageDto
                            {
                                Id = index + 1,
                                Type = GetMessageType(log.Action),
                                Title = GetMessageTitle(log.Action, log.EntityType ?? ""),
                                SubTitle = "",
                                Avatar = "",
                                Content = log.Description ?? $"{log.Action} {log.EntityType}",
                                Time = FormatTime(log.CreatedAt),
                                Status = 0, // Unread by default
                                MessageType = GetMessageTypeCode(log.Action),
                            }
                    )
                    .ToList();

                return Ok(new { code = 20000, data = messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message list");
                return Ok(new { code = 20000, data = new List<MessageDto>() }); // Return empty list on error
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPost("read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult MarkAsRead([FromBody] MarkReadRequest request)
        {
            // For now, just return success
            // In a real implementation, you would store read status in the database
            return Ok(new { code = 20000, data = true });
        }

        private static string GetMessageType(string action)
        {
            return action switch
            {
                "Create" or "Update" or "Delete" => "notice",
                "Login" or "Logout" => "message",
                _ => "todo",
            };
        }

        private static string GetMessageTitle(string action, string entityType)
        {
            return action switch
            {
                "Create" => $"Created {entityType}",
                "Update" => $"Updated {entityType}",
                "Delete" => $"Deleted {entityType}",
                "Login" => "User Login",
                "Logout" => "User Logout",
                _ => $"{action} {entityType}",
            };
        }

        private static int GetMessageTypeCode(string action)
        {
            return action switch
            {
                "Create" => 1,
                "Update" => 2,
                "Delete" => 3,
                _ => 0,
            };
        }

        private static string FormatTime(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            var diff = now - dateTime;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";

            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string SubTitle { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Content { get; set; } = "";
        public string Time { get; set; } = "";
        public int Status { get; set; }
        public int MessageType { get; set; }
    }

    public class MarkReadRequest
    {
        public int[] Ids { get; set; } = [];
    }
}
