using System;
namespace Dawning.Auth.Domain.Enums
{
    /// <summary>
    /// Token expiration types.
    /// </summary>
    public enum TokenExpiration
    {
        /// <summary>
        /// Sliding token expiration
        /// </summary>
        Sliding,
        /// <summary>
        /// Absolute token expiration
        /// </summary>
        Absolute
    }
}

