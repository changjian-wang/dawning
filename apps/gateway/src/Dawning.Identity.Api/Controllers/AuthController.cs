using System.Security.Claims;
using Dawning.Identity.Api.Models;
using Dawning.Identity.Application.Interfaces.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Dawning.Identity.Api.Controllers
{
    /// <summary>
    /// Authentication controller - handles OAuth 2.0 / OpenID Connect authentication flow
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService;

        public AuthController(IUserAuthenticationService userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        /// <summary>
        /// Token endpoint - handles various OAuth 2.0 grant types
        /// Supports: password, client_credentials, authorization_code, refresh_token
        /// </summary>
        [HttpPost("~/connect/token")]
        [Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request =
                HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenID Connect request cannot be retrieved."
                );

            // Handle password grant flow (Resource Owner Password Credentials)
            if (request.IsPasswordGrantType())
            {
                return await HandlePasswordGrantAsync(request);
            }

            // Handle client credentials grant flow (Client Credentials)
            if (request.IsClientCredentialsGrantType())
            {
                return await HandleClientCredentialsGrantAsync(request);
            }

            // Handle authorization code grant flow (Authorization Code)
            if (request.IsAuthorizationCodeGrantType())
            {
                return await HandleAuthorizationCodeGrantAsync();
            }

            // Handle refresh token grant flow (Refresh Token)
            if (request.IsRefreshTokenGrantType())
            {
                return await HandleRefreshTokenGrantAsync();
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        /// <summary>
        /// Authorization endpoint - for authorization code flow
        /// </summary>
        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request =
                HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenID Connect request cannot be retrieved."
                );

            // Check if user is authenticated
            var result = await HttpContext.AuthenticateAsync();
            if (!result.Succeeded || request.HasPrompt(Prompts.Login))
            {
                // If user is not authenticated, return challenge response
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri =
                            Request.PathBase
                            + Request.Path
                            + QueryString.Create(
                                Request.HasFormContentType
                                    ? Request.Form.ToList()
                                    : Request.Query.ToList()
                            ),
                    }
                );
            }

            // Create user identity claims
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // Get user info from authentication result
            identity
                .SetClaim(
                    Claims.Subject,
                    result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty
                )
                .SetClaim(
                    Claims.Name,
                    result.Principal?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty
                )
                .SetClaim(
                    Claims.Email,
                    result.Principal?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty
                );

            identity.SetDestinations(GetDestinations);

            // Return authorization response
            return SignIn(
                new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// UserInfo endpoint - returns current user information
        /// </summary>
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        [HttpPost("~/connect/userinfo")]
        [Produces("application/json")]
        public async Task<IActionResult> UserInfo()
        {
            var claimsPrincipal = (
                await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )
            ).Principal;

            if (claimsPrincipal == null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(
                        new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                Errors.InvalidToken,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The specified access token is invalid.",
                        }
                    )
                );
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Required claims
                [Claims.Subject] = claimsPrincipal.GetClaim(Claims.Subject) ?? string.Empty,
            };

            // Optional claims
            if (claimsPrincipal.HasScope(Scopes.Profile))
            {
                claims[Claims.Name] = claimsPrincipal.GetClaim(Claims.Name) ?? string.Empty;
            }

            if (claimsPrincipal.HasScope(Scopes.Email))
            {
                claims[Claims.Email] = claimsPrincipal.GetClaim(Claims.Email) ?? string.Empty;
                claims[Claims.EmailVerified] = false;
            }

            return Ok(claims);
        }

        /// <summary>
        /// Logout endpoint
        /// For pure API OAuth 2.0 server (no server-side session), logout only requires client to clear local token
        /// If server-side token revocation is needed, use the /connect/revoke endpoint
        /// </summary>
        [HttpGet("~/connect/logout")]
        [HttpPost("~/connect/logout")]
        public IActionResult Logout()
        {
            // OAuth 2.0 API server is stateless, no server-side session needs to be cleared
            // Client logout only needs to clear locally stored access_token, refresh_token, etc.
            // If token revocation is needed to invalidate immediately, call the /connect/revoke endpoint
            return Ok(ApiResponse.Success("Logged out successfully"));
        }

        #region Private helper methods

        /// <summary>
        /// Handle password grant flow
        /// </summary>
        private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
        {
            // Validate user credentials
            var user = await _userAuthenticationService.ValidateCredentialsAsync(
                request.Username ?? string.Empty,
                request.Password ?? string.Empty
            );

            // Check if account is locked out
            if (user != null && user.IsLockedOut)
            {
                var lockoutProperties = new AuthenticationProperties(
                    new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            Errors.AccessDenied,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            user.LockoutMessage
                            ?? "Account is locked out due to too many failed login attempts.",
                    }
                );

                return Forbid(
                    lockoutProperties,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                );
            }

            if (user == null || string.IsNullOrEmpty(user.Id))
            {
                var properties = new AuthenticationProperties(
                    new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The username/password couple is invalid.",
                    }
                );

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Create user identity
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity
                .SetClaim(Claims.Subject, user.Id)
                .SetClaim(Claims.Name, user.Username)
                .SetClaim(Claims.Email, user.Email ?? string.Empty);

            // Add all user roles as role claims
            foreach (var role in user.Roles)
            {
                identity.AddClaim(new Claim(Claims.Role, role));
            }

            // Set scopes
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(
                new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// Handle client credentials grant flow
        /// </summary>
        private Task<IActionResult> HandleClientCredentialsGrantAsync(OpenIddictRequest request)
        {
            // Create client identity
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity
                .SetClaim(Claims.Subject, request.ClientId ?? string.Empty)
                .SetClaim(Claims.Name, request.ClientId ?? string.Empty);

            // Set scopes
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return Task.FromResult<IActionResult>(
                SignIn(
                    new ClaimsPrincipal(identity),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )
            );
        }

        /// <summary>
        /// Handle authorization code grant flow
        /// </summary>
        private async Task<IActionResult> HandleAuthorizationCodeGrantAsync()
        {
            var claimsPrincipal = (
                await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )
            ).Principal;

            if (claimsPrincipal == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(
                        new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The token is no longer valid.",
                        }
                    )
                );
            }

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handle refresh token grant flow
        /// </summary>
        private async Task<IActionResult> HandleRefreshTokenGrantAsync()
        {
            var claimsPrincipal = (
                await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )
            ).Principal;

            if (claimsPrincipal == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(
                        new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The refresh token is no longer valid.",
                        }
                    )
                );
            }

            // Verify user still exists
            var userId = claimsPrincipal.GetClaim(Claims.Subject);
            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(
                        new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The user no longer exists.",
                        }
                    )
                );
            }

            var identity = new ClaimsIdentity(
                claimsPrincipal.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity.SetDestinations(GetDestinations);

            return SignIn(
                new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// Determine claim destinations (access_token and/or id_token)
        /// </summary>
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: By default, claims are not automatically included in access tokens and identity tokens
            // Must explicitly specify claim destinations

            switch (claim.Type)
            {
                // These claims should be included in both access token and identity token
                case Claims.Name:
                case Claims.Subject:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    yield break;

                // These claims should only be included in identity token
                case Claims.Email:
                case Claims.EmailVerified:
                    yield return Destinations.IdentityToken;
                    yield break;

                // Role claims should only be included in access token
                case Claims.Role:
                    yield return Destinations.AccessToken;
                    yield break;

                // Other claims are not included by default
                default:
                    yield break;
            }
        }

        #endregion
    }
}
