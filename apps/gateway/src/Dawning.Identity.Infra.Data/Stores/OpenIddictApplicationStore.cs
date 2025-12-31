using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace Dawning.Identity.Infra.Data.Stores
{
    /// <summary>
    /// OpenIddict Application Store - bridges to Dapper Repository
    /// </summary>
    public class OpenIddictApplicationStore : IOpenIddictApplicationStore<Application>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenIddictApplicationStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Get total application count
        /// </summary>
        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Application.GetAllAsync();
            return all.Count();
        }

        /// <summary>
        /// Count by condition
        /// </summary>
        public ValueTask<long> CountAsync<TResult>(
            Func<IQueryable<Application>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            // Dapper does not support IQueryable, using simple count
            return CountAsync(cancellationToken);
        }

        /// <summary>
        /// Create new application
        /// </summary>
        public async ValueTask CreateAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Ensure ID exists
            if (application.Id == Guid.Empty)
                application.Id = Guid.NewGuid();

            // Set timestamp
            application.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Application.InsertAsync(application);
        }

        /// <summary>
        /// Delete application
        /// </summary>
        public async ValueTask DeleteAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            await _unitOfWork.Application.DeleteAsync(application);
        }

        /// <summary>
        /// Find application by Client ID
        /// </summary>
        public async ValueTask<Application?> FindByClientIdAsync(
            string identifier,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            return await _unitOfWork.Application.GetByClientIdAsync(identifier);
        }

        /// <summary>
        /// Find application by ID
        /// </summary>
        public async ValueTask<Application?> FindByIdAsync(
            string identifier,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            // OpenIddict uses string ID, we use Guid
            if (!Guid.TryParse(identifier, out var guid))
                return null;

            var app = await _unitOfWork.Application.GetAsync(guid);
            return app.Id == Guid.Empty ? null : app;
        }

        /// <summary>
        /// Find application by Post Logout Redirect URI
        /// </summary>
        public async IAsyncEnumerable<Application> FindByPostLogoutRedirectUriAsync(
            string uri,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(uri))
                yield break;

            var all = await _unitOfWork.Application.GetAllAsync();
            var filtered = all.Where(a =>
                a.PostLogoutRedirectUris != null && a.PostLogoutRedirectUris.Contains(uri)
            );

            foreach (var app in filtered)
            {
                yield return app;
            }
        }

        /// <summary>
        /// Find application by Redirect URI
        /// </summary>
        public async IAsyncEnumerable<Application> FindByRedirectUriAsync(
            string uri,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(uri))
                yield break;

            var all = await _unitOfWork.Application.GetAllAsync();
            var filtered = all.Where(a => a.RedirectUris != null && a.RedirectUris.Contains(uri));

            foreach (var app in filtered)
            {
                yield return app;
            }
        }

        /// <summary>
        /// Get Client ID
        /// </summary>
        public ValueTask<string?> GetClientIdAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.ClientId);
        }

        /// <summary>
        /// Get Client Secret
        /// </summary>
        public ValueTask<string?> GetClientSecretAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.ClientSecret);
        }

        /// <summary>
        /// Get Client Type
        /// </summary>
        public ValueTask<string?> GetClientTypeAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.Type);
        }

        /// <summary>
        /// Get Consent Type
        /// </summary>
        public ValueTask<string?> GetConsentTypeAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.ConsentType);
        }

        /// <summary>
        /// Get Display Name
        /// </summary>
        public ValueTask<string?> GetDisplayNameAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.DisplayName);
        }

        /// <summary>
        /// Get Display Names (localized)
        /// </summary>
        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Simple implementation: only return default DisplayName
            var dict = ImmutableDictionary.CreateBuilder<CultureInfo, string>();
            if (!string.IsNullOrEmpty(application.DisplayName))
            {
                dict.Add(CultureInfo.InvariantCulture, application.DisplayName);
            }

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(dict.ToImmutable());
        }

        /// <summary>
        /// Get application ID
        /// </summary>
        public ValueTask<string?> GetIdAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<string?>(application.Id.ToString());
        }

        /// <summary>
        /// Get permissions list
        /// </summary>
        public ValueTask<ImmutableArray<string>> GetPermissionsAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<ImmutableArray<string>>(
                application.Permissions?.ToImmutableArray() ?? ImmutableArray<string>.Empty
            );
        }

        /// <summary>
        /// Get Post Logout Redirect URIs
        /// </summary>
        public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<ImmutableArray<string>>(
                application.PostLogoutRedirectUris?.ToImmutableArray()
                    ?? ImmutableArray<string>.Empty
            );
        }

        /// <summary>
        /// Get extended properties
        /// </summary>
        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();
            if (application.Properties != null)
            {
                foreach (var prop in application.Properties)
                {
                    builder.Add(prop.Key, JsonSerializer.SerializeToElement(prop.Value));
                }
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(builder.ToImmutable());
        }

        /// <summary>
        /// Get Redirect URIs
        /// </summary>
        public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<ImmutableArray<string>>(
                application.RedirectUris?.ToImmutableArray() ?? ImmutableArray<string>.Empty
            );
        }

        /// <summary>
        /// Get Requirements
        /// </summary>
        public ValueTask<ImmutableArray<string>> GetRequirementsAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return new ValueTask<ImmutableArray<string>>(
                application.Requirements?.ToImmutableArray() ?? ImmutableArray<string>.Empty
            );
        }

        /// <summary>
        /// Get Settings (OpenIddict 5.x new feature)
        /// </summary>
        public ValueTask<ImmutableDictionary<string, string>> GetSettingsAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Extract settings from Properties
            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            if (application.Properties != null)
            {
                foreach (var prop in application.Properties)
                {
                    builder.Add(prop.Key, prop.Value);
                }
            }

            return new ValueTask<ImmutableDictionary<string, string>>(builder.ToImmutable());
        }

        /// <summary>
        /// Instantiate new application
        /// </summary>
        public ValueTask<Application> InstantiateAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<Application>(
                new Application { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
        }

        /// <summary>
        /// List applications
        /// </summary>
        public async IAsyncEnumerable<Application> ListAsync(
            int? count,
            int? offset,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Application.GetAllAsync();
            var query = all.AsEnumerable();

            if (offset.HasValue)
                query = query.Skip(offset.Value);

            if (count.HasValue)
                query = query.Take(count.Value);

            foreach (var app in query)
            {
                yield return app;
            }
        }

        /// <summary>
        /// List applications by query
        /// </summary>
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            // Dapper does not support IQueryable, return empty list
            await Task.CompletedTask;
            yield break;
        }

        /// <summary>
        /// Set Client ID
        /// </summary>
        public ValueTask SetClientIdAsync(
            Application application,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.ClientId = identifier;
            return default;
        }

        /// <summary>
        /// Set Client Secret
        /// </summary>
        public ValueTask SetClientSecretAsync(
            Application application,
            string? secret,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.ClientSecret = secret;
            return default;
        }

        /// <summary>
        /// Set Client Type
        /// </summary>
        public ValueTask SetClientTypeAsync(
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.Type = type;
            return default;
        }

        /// <summary>
        /// Set Consent Type
        /// </summary>
        public ValueTask SetConsentTypeAsync(
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.ConsentType = type;
            return default;
        }

        /// <summary>
        /// Set Display Name
        /// </summary>
        public ValueTask SetDisplayNameAsync(
            Application application,
            string? name,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.DisplayName = name;
            return default;
        }

        /// <summary>
        /// Set Display Names (localized)
        /// </summary>
        public ValueTask SetDisplayNamesAsync(
            Application application,
            ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Simple implementation: only use InvariantCulture value
            if (names.TryGetValue(CultureInfo.InvariantCulture, out var name))
            {
                application.DisplayName = name;
            }

            return default;
        }

        /// <summary>
        /// Set permissions list
        /// </summary>
        public ValueTask SetPermissionsAsync(
            Application application,
            ImmutableArray<string> permissions,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.Permissions = permissions.ToList();
            return default;
        }

        /// <summary>
        /// Set Post Logout Redirect URIs
        /// </summary>
        public ValueTask SetPostLogoutRedirectUrisAsync(
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.PostLogoutRedirectUris = uris.ToList();
            return default;
        }

        /// <summary>
        /// Set extended properties
        /// </summary>
        public ValueTask SetPropertiesAsync(
            Application application,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.Properties = properties.ToDictionary(
                p => p.Key,
                p => p.Value.GetString() ?? string.Empty
            );

            return default;
        }

        /// <summary>
        /// Set Redirect URIs
        /// </summary>
        public ValueTask SetRedirectUrisAsync(
            Application application,
            ImmutableArray<string> uris,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.RedirectUris = uris.ToList();
            return default;
        }

        /// <summary>
        /// Set Requirements
        /// </summary>
        public ValueTask SetRequirementsAsync(
            Application application,
            ImmutableArray<string> requirements,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.Requirements = requirements.ToList();
            return default;
        }

        /// <summary>
        /// Set Settings
        /// </summary>
        public ValueTask SetSettingsAsync(
            Application application,
            ImmutableDictionary<string, string> settings,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Save to Properties
            application.Properties = settings.ToDictionary(s => s.Key, s => s.Value);
            return default;
        }

        /// <summary>
        /// Update application
        /// </summary>
        public async ValueTask UpdateAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Application.UpdateAsync(application);
        }

        /// <summary>
        /// Get Application Type (OpenIddict 5.x new feature)
        /// </summary>
        public ValueTask<string?> GetApplicationTypeAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Application Type corresponds to our Type field
            return new ValueTask<string?>(application.Type);
        }

        /// <summary>
        /// Set Application Type
        /// </summary>
        public ValueTask SetApplicationTypeAsync(
            Application application,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.Type = type;
            return default;
        }

        /// <summary>
        /// Get JsonWebKeySet
        /// </summary>
        public ValueTask<JsonWebKeySet?> GetJsonWebKeySetAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Simple implementation: JWKS not supported, return null
            return new ValueTask<JsonWebKeySet?>(default(JsonWebKeySet));
        }

        /// <summary>
        /// Set JsonWebKeySet
        /// </summary>
        public ValueTask SetJsonWebKeySetAsync(
            Application application,
            JsonWebKeySet? set,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Simple implementation: JWKS not supported
            return default;
        }

        /// <summary>
        /// GetAsync - for querying
        /// </summary>
        public ValueTask<TResult?> GetAsync<TState, TResult>(
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            // Dapper does not support IQueryable
            return new ValueTask<TResult?>(default(TResult));
        }
    }
}
