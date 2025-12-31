using Dawning.Identity.Application.Dtos.OpenIddict;
using Dawning.Identity.Application.Interfaces.OpenIddict;
using Dawning.Identity.Domain.Core.Security;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Dawning.Identity.Api.Data
{
    /// <summary>
    /// Database seed data initialization
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly IApplicationService _applicationService;
        private readonly IScopeService _scopeService;
        private readonly Dawning.Identity.Application.Interfaces.Administration.IUserService _userService;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            IApplicationService applicationService,
            IScopeService scopeService,
            Dawning.Identity.Application.Interfaces.Administration.IUserService userService,
            ILogger<DatabaseSeeder> logger
        )
        {
            _applicationService = applicationService;
            _scopeService = scopeService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Initialize seed data
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                await SeedScopesAsync();
                await SeedApplicationsAsync();
                await SeedUsersAsync();
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }

        /// <summary>
        /// Initialize scopes
        /// </summary>
        private async Task SeedScopesAsync()
        {
            var scopes = new[]
            {
                new
                {
                    Name = Scopes.OpenId,
                    DisplayName = "OpenID",
                    Description = "OpenID Connect scope",
                },
                new
                {
                    Name = Scopes.Profile,
                    DisplayName = "Profile",
                    Description = "User profile information",
                },
                new
                {
                    Name = Scopes.Email,
                    DisplayName = "Email",
                    Description = "User email address",
                },
                new
                {
                    Name = Scopes.Roles,
                    DisplayName = "Roles",
                    Description = "User roles",
                },
                new
                {
                    Name = "api",
                    DisplayName = "API",
                    Description = "API access scope",
                },
            };

            foreach (var scope in scopes)
            {
                var existingScope = await _scopeService.GetByNameAsync(scope.Name);
                if (existingScope == null)
                {
                    var newScope = new ScopeDto
                    {
                        Id = Guid.NewGuid(),
                        Name = scope.Name,
                        DisplayName = scope.DisplayName,
                        Description = scope.Description,
                        Resources =
                            scope.Name == "api"
                                ? new List<string> { "api-resource" }
                                : new List<string>(),
                        Properties = new Dictionary<string, string>(),
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        CreatedAt = DateTime.UtcNow,
                    };

                    await _scopeService.InsertAsync(newScope);
                    _logger.LogInformation("Scope '{ScopeName}' created", scope.Name);
                }
                else
                {
                    _logger.LogInformation("Scope '{ScopeName}' already exists", scope.Name);
                }
            }
        }

        /// <summary>
        /// Initialize applications
        /// </summary>
        private async Task SeedApplicationsAsync()
        {
            // Dawning Admin frontend application
            var adminClientId = "dawning-admin";
            var existingAdminApp = await _applicationService.GetByClientIdAsync(adminClientId);

            if (existingAdminApp == null)
            {
                var adminApp = new ApplicationDto
                {
                    Id = Guid.NewGuid(),
                    ClientId = adminClientId,
                    ClientSecret = null, // Public client doesn't need secret (SPA application)
                    DisplayName = "Dawning Admin",
                    Type = ClientTypes.Public, // SPA application uses Public type
                    ConsentType = ConsentTypes.Implicit, // Implicit consent
                    Permissions = new List<string>
                    {
                        // Grant types
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.GrantTypes.AuthorizationCode,
                        // Endpoints
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        // Scopes
                        Permissions.Prefixes.Scope + Scopes.OpenId,
                        Permissions.Prefixes.Scope + Scopes.Profile,
                        Permissions.Prefixes.Scope + Scopes.Email,
                        Permissions.Prefixes.Scope + Scopes.Roles,
                        Permissions.Prefixes.Scope + "api",
                    },
                    RedirectUris = new List<string>
                    {
                        "http://localhost:5173/callback",
                        "http://localhost:5173/silent-renew",
                        "https://localhost:5173/callback",
                        "https://localhost:5173/silent-renew",
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:5173/login",
                        "https://localhost:5173/login",
                    },
                    Requirements = new List<string>(),
                    Properties = new Dictionary<string, string>(),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    CreatedAt = DateTime.UtcNow,
                };

                await _applicationService.InsertAsync(adminApp);
                _logger.LogInformation("Application '{ClientId}' created", adminClientId);
            }
            else
            {
                _logger.LogInformation("Application '{ClientId}' already exists", adminClientId);
            }

            // API service (for service-to-service calls)
            var apiClientId = "dawning-api";
            var existingApiApp = await _applicationService.GetByClientIdAsync(apiClientId);

            if (existingApiApp == null)
            {
                var apiApp = new ApplicationDto
                {
                    Id = Guid.NewGuid(),
                    ClientId = apiClientId,
                    ClientSecret = PasswordHasher.Hash("dawning-api-secret"), // Using PBKDF2 hash
                    DisplayName = "Dawning API",
                    Type = ClientTypes.Confidential, // Server-side application uses Confidential type
                    ConsentType = ConsentTypes.Implicit,
                    Permissions = new List<string>
                    {
                        // Grant types
                        Permissions.GrantTypes.ClientCredentials,
                        // Endpoints
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Introspection,
                        // Scopes
                        Permissions.Prefixes.Scope + "api",
                    },
                    RedirectUris = new List<string>(),
                    PostLogoutRedirectUris = new List<string>(),
                    Requirements = new List<string>(),
                    Properties = new Dictionary<string, string>(),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    CreatedAt = DateTime.UtcNow,
                };

                await _applicationService.InsertAsync(apiApp);
                _logger.LogInformation("Application '{ClientId}' created", apiClientId);
            }
            else
            {
                _logger.LogInformation("Application '{ClientId}' already exists", apiClientId);
            }
        }

        /// <summary>
        /// Initialize default users
        /// </summary>
        private async Task SeedUsersAsync()
        {
            // Check if any users already exist in system
            var allUsersModel = new Dawning.Identity.Domain.Models.Administration.UserModel();

            var existingUsers = await _userService.GetPagedListAsync(allUsersModel, 1, 1);
            if (existingUsers.TotalCount > 0)
            {
                _logger.LogInformation(
                    "Users already exist in system, skipping default user creation"
                );
                _logger.LogInformation(
                    "To initialize super admin account, please call POST /api/user/initialize-admin"
                );
                return;
            }

            _logger.LogInformation(
                "No users found in system. Use POST /api/user/initialize-admin to create super admin account"
            );
        }
    }
}
