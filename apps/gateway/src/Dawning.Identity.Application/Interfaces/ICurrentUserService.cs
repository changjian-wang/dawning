using System;

namespace Dawning.Identity.Application.Interfaces
{
    /// <summary>
    /// Current user service interface
    /// Used to get current logged-in user information in the Application layer
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user ID
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the current username
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Whether the user is authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the list of user roles
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        /// Checks whether the user has a specified role
        /// </summary>
        bool IsInRole(string role);
    }
}
