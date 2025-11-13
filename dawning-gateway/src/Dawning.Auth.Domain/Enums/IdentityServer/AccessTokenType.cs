using System;
namespace Dawning.Auth.Domain.Enums
{
    /// <summary>
    /// Access token types.
    /// </summary>
    public enum AccessTokenType
    {
        /// <summary>
        /// Self-contained Json Web Token
        /// </summary>
        Jwt,
        /// <summary>
        /// Reference token
        /// </summary>
        Reference
    }
}

