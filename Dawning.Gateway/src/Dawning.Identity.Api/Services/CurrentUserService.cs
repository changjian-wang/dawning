using System;
using System.Collections.Generic;
using System.Security.Claims;
using Dawning.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Dawning.Identity.Api.Services
{
    /// <summary>
    /// 当前用户服务实现
    /// 从 HttpContext 获取当前登录用户信息
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
        /// 获取当前用户ID
        /// </summary>
        public Guid? UserId
        {
            get
            {
                if (User == null || !User.Identity?.IsAuthenticated == true)
                    return null;

                // 尝试从多个声明类型获取用户ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
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
        /// 获取当前用户名
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
        /// 是否已认证
        /// </summary>
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// 获取用户角色列表
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
        /// 检查用户是否拥有指定角色
        /// </summary>
        public bool IsInRole(string role)
        {
            return User?.IsInRole(role) ?? false;
        }
    }
}
