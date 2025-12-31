using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Dawning.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Application.Services
{
    /// <summary>
    /// Current user service implementation
    /// Gets current logged-in user information from HttpContext
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        /// <summary>
        /// Get current user ID
        /// </summary>
        public Guid? UserId
        {
            get
            {
                if (User == null || !User.Identity?.IsAuthenticated == true)
                    return null;

                // Try to get user ID from multiple claim types
                var userIdClaim =
                    User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub")
                    ?? User.FindFirst("user_id");

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }

        /// <summary>
        /// Get current username
        /// </summary>
        public string? Username
        {
            get
            {
                if (User == null || !User.Identity?.IsAuthenticated == true)
                    return null;

                return User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst("username")?.Value
                    ?? User.Identity?.Name;
            }
        }

        /// <summary>
        /// Whether authenticated
        /// </summary>
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Get user role list
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                if (User == null)
                    return Enumerable.Empty<string>();

                return User.FindAll(ClaimTypes.Role)
                    .Concat(User.FindAll("role"))
                    .Select(c => c.Value)
                    .Distinct();
            }
        }

        /// <summary>
        /// Check if user has specified role
        /// </summary>
        public bool IsInRole(string role)
        {
            return User?.IsInRole(role) ?? false;
        }
    }
}
