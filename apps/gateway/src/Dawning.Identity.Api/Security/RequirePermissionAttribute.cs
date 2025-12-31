using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// Permission validation attribute, used to mark API endpoints that require specific permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Permission code prefix
        /// </summary>
        public const string PolicyPrefix = "Permission:";

        /// <summary>
        /// Permission code
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Create permission validation attribute
        /// </summary>
        /// <param name="permission">Permission code, e.g., "user.create", "role.delete"</param>
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
            Policy = $"{PolicyPrefix}{permission}";
        }
    }
}
