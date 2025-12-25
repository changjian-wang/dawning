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
    /// OpenIddict Application Store - 桥接到 Dapper Repository
    /// </summary>
    public class OpenIddictApplicationStore : IOpenIddictApplicationStore<Application>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenIddictApplicationStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 获取应用程序总数
        /// </summary>
        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Application.GetAllAsync();
            return all.Count();
        }

        /// <summary>
        /// 根据条件统计数量
        /// </summary>
        public ValueTask<long> CountAsync<TResult>(
            Func<IQueryable<Application>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            // Dapper 不支持 IQueryable，使用简单计数
            return CountAsync(cancellationToken);
        }

        /// <summary>
        /// 创建新应用程序
        /// </summary>
        public async ValueTask CreateAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 确保有 ID
            if (application.Id == Guid.Empty)
                application.Id = Guid.NewGuid();

            // 设置时间戳
            application.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Application.InsertAsync(application);
        }

        /// <summary>
        /// 删除应用程序
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
        /// 根据 Client ID 查找应用程序
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
        /// 根据 ID 查找应用程序
        /// </summary>
        public async ValueTask<Application?> FindByIdAsync(
            string identifier,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            // OpenIddict 使用 string ID，我们使用 Guid
            if (!Guid.TryParse(identifier, out var guid))
                return null;

            var app = await _unitOfWork.Application.GetAsync(guid);
            return app.Id == Guid.Empty ? null : app;
        }

        /// <summary>
        /// 根据 Post Logout Redirect URI 查找应用程序
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
        /// 根据 Redirect URI 查找应用程序
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
        /// 获取 Client ID
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
        /// 获取 Client Secret
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
        /// 获取 Client Type
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
        /// 获取 Consent Type
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
        /// 获取 Display Name
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
        /// 获取 Display Names (本地化)
        /// </summary>
        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 简单实现：只返回默认的 DisplayName
            var dict = ImmutableDictionary.CreateBuilder<CultureInfo, string>();
            if (!string.IsNullOrEmpty(application.DisplayName))
            {
                dict.Add(CultureInfo.InvariantCulture, application.DisplayName);
            }

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(dict.ToImmutable());
        }

        /// <summary>
        /// 获取应用程序 ID
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
        /// 获取权限列表
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
        /// 获取 Post Logout Redirect URIs
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
        /// 获取扩展属性
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
        /// 获取 Redirect URIs
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
        /// 获取 Requirements
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
        /// 获取 Settings (OpenIddict 5.x 新增)
        /// </summary>
        public ValueTask<ImmutableDictionary<string, string>> GetSettingsAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 从 Properties 中提取 settings
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
        /// 实例化新应用程序
        /// </summary>
        public ValueTask<Application> InstantiateAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<Application>(
                new Application { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
        }

        /// <summary>
        /// 列出应用程序
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
        /// 根据查询列出应用程序
        /// </summary>
        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            // Dapper 不支持 IQueryable，返回空列表
            await Task.CompletedTask;
            yield break;
        }

        /// <summary>
        /// 设置 Client ID
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
        /// 设置 Client Secret
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
        /// 设置 Client Type
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
        /// 设置 Consent Type
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
        /// 设置 Display Name
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
        /// 设置 Display Names (本地化)
        /// </summary>
        public ValueTask SetDisplayNamesAsync(
            Application application,
            ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 简单实现：只使用 InvariantCulture 的值
            if (names.TryGetValue(CultureInfo.InvariantCulture, out var name))
            {
                application.DisplayName = name;
            }

            return default;
        }

        /// <summary>
        /// 设置权限列表
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
        /// 设置 Post Logout Redirect URIs
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
        /// 设置扩展属性
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
        /// 设置 Redirect URIs
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
        /// 设置 Requirements
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
        /// 设置 Settings
        /// </summary>
        public ValueTask SetSettingsAsync(
            Application application,
            ImmutableDictionary<string, string> settings,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 保存到 Properties
            application.Properties = settings.ToDictionary(s => s.Key, s => s.Value);
            return default;
        }

        /// <summary>
        /// 更新应用程序
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
        /// 获取 Application Type (OpenIddict 5.x 新增)
        /// </summary>
        public ValueTask<string?> GetApplicationTypeAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Application Type 对应我们的 Type 字段
            return new ValueTask<string?>(application.Type);
        }

        /// <summary>
        /// 设置 Application Type
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
        /// 获取 JsonWebKeySet
        /// </summary>
        public ValueTask<JsonWebKeySet?> GetJsonWebKeySetAsync(
            Application application,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 简单实现：不支持 JWKS，返回 null
            return new ValueTask<JsonWebKeySet?>(default(JsonWebKeySet));
        }

        /// <summary>
        /// 设置 JsonWebKeySet
        /// </summary>
        public ValueTask SetJsonWebKeySetAsync(
            Application application,
            JsonWebKeySet? set,
            CancellationToken cancellationToken
        )
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // 简单实现：不支持 JWKS
            return default;
        }

        /// <summary>
        /// GetAsync - 用于查询
        /// </summary>
        public ValueTask<TResult?> GetAsync<TState, TResult>(
            Func<IQueryable<Application>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            // Dapper 不支持 IQueryable
            return new ValueTask<TResult?>(default(TResult));
        }
    }
}
