using Microsoft.AspNetCore.Authorization;

namespace Dawning.Identity.Api.Security
{
    /// <summary>
    /// Permission requirement, used to define permissions to be validated
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Required permission code
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Create permission requirement
        /// </summary>
        /// <param name="permission">Permission code</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
