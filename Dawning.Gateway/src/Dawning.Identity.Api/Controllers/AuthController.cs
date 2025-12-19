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
    /// 认证控制器 - 处理 OAuth 2.0 / OpenID Connect 认证流程
    /// </summary>
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthenticationService _userAuthenticationService;

        public AuthController(IUserAuthenticationService userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        /// <summary>
        /// Token 端点 - 处理各种 OAuth 2.0 授权类型
        /// 支持：password、client_credentials、authorization_code、refresh_token
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

            // 处理密码授权流程 (Resource Owner Password Credentials)
            if (request.IsPasswordGrantType())
            {
                return await HandlePasswordGrantAsync(request);
            }

            // 处理客户端凭证流程 (Client Credentials)
            if (request.IsClientCredentialsGrantType())
            {
                return await HandleClientCredentialsGrantAsync(request);
            }

            // 处理授权码流程 (Authorization Code)
            if (request.IsAuthorizationCodeGrantType())
            {
                return await HandleAuthorizationCodeGrantAsync();
            }

            // 处理刷新令牌流程 (Refresh Token)
            if (request.IsRefreshTokenGrantType())
            {
                return await HandleRefreshTokenGrantAsync();
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        /// <summary>
        /// 授权端点 - 用于授权码流程
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

            // 检查用户是否已认证
            var result = await HttpContext.AuthenticateAsync();
            if (!result.Succeeded || request.HasPrompt(Prompts.Login))
            {
                // 如果用户未认证，返回挑战响应
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

            // 创建用户身份声明
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            // 从认证结果中获取用户信息
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

            // 返回授权响应
            return SignIn(
                new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// 用户信息端点 - 返回当前用户的信息
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
                // 必需声明
                [Claims.Subject] = claimsPrincipal.GetClaim(Claims.Subject) ?? string.Empty,
            };

            // 可选声明
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
        /// 登出端点
        /// 对于纯 API OAuth 2.0 服务器（无服务端 session），登出只需要客户端清除本地 token
        /// 如果需要服务端撤销 token，应使用 /connect/revoke 端点
        /// </summary>
        [HttpGet("~/connect/logout")]
        [HttpPost("~/connect/logout")]
        public IActionResult Logout()
        {
            // OAuth 2.0 API 服务器是无状态的，没有服务端 session 需要清除
            // 客户端登出只需清除本地存储的 access_token、refresh_token 等
            // 如果需要撤销 token 使其立即失效，应调用 /connect/revoke 端点
            return Ok(ApiResponse.Success("Logged out successfully"));
        }

        #region 私有辅助方法

        /// <summary>
        /// 处理密码授权流程
        /// </summary>
        private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
        {
            // 验证用户凭据
            var user = await _userAuthenticationService.ValidateCredentialsAsync(
                request.Username ?? string.Empty,
                request.Password ?? string.Empty
            );

            // 检查是否账户被锁定
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

            // 创建用户身份
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity
                .SetClaim(Claims.Subject, user.Id)
                .SetClaim(Claims.Name, user.Username)
                .SetClaim(Claims.Email, user.Email ?? string.Empty);

            // 添加所有用户角色作为 role claims
            foreach (var role in user.Roles)
            {
                identity.AddClaim(new Claim(Claims.Role, role));
            }

            // 设置作用域
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(
                new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );
        }

        /// <summary>
        /// 处理客户端凭证流程
        /// </summary>
        private Task<IActionResult> HandleClientCredentialsGrantAsync(OpenIddictRequest request)
        {
            // 创建客户端身份
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity
                .SetClaim(Claims.Subject, request.ClientId ?? string.Empty)
                .SetClaim(Claims.Name, request.ClientId ?? string.Empty);

            // 设置作用域
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
        /// 处理授权码流程
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
        /// 处理刷新令牌流程
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

            // 验证用户是否仍然存在
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
        /// 确定声明的目标（access_token 和/或 id_token）
        /// </summary>
        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // 注意: 默认情况下，声明不会自动包含在访问令牌和身份令牌中
            // 必须显式指定声明的目标

            switch (claim.Type)
            {
                // 这些声明应该包含在访问令牌和身份令牌中
                case Claims.Name:
                case Claims.Subject:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    yield break;

                // 这些声明只应包含在身份令牌中
                case Claims.Email:
                case Claims.EmailVerified:
                    yield return Destinations.IdentityToken;
                    yield break;

                // 角色声明只应包含在访问令牌中
                case Claims.Role:
                    yield return Destinations.AccessToken;
                    yield break;

                // 其他声明默认不包含
                default:
                    yield break;
            }
        }

        #endregion
    }
}
