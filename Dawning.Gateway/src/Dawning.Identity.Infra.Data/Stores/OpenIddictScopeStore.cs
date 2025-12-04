using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using OpenIddict.Abstractions;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Dawning.Identity.Infra.Data.Stores
{
    /// <summary>
    /// OpenIddict Scope Store - 桥接到 Dapper Repository
    /// </summary>
    public class OpenIddictScopeStore : IOpenIddictScopeStore<Scope>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenIddictScopeStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Scope.GetAllAsync();
            return all.Count();
        }

        public ValueTask<long> CountAsync<TResult>(Func<IQueryable<Scope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            return CountAsync(cancellationToken);
        }

        public async ValueTask CreateAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (scope.Id == Guid.Empty)
                scope.Id = Guid.NewGuid();

            scope.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Scope.InsertAsync(scope);
        }

        public async ValueTask DeleteAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            await _unitOfWork.Scope.DeleteAsync(scope);
        }

        public async ValueTask<Scope?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            if (!Guid.TryParse(identifier, out var guid))
                return null;

            var scope = await _unitOfWork.Scope.GetAsync(guid);
            return scope.Id == Guid.Empty ? null : scope;
        }

        public async ValueTask<Scope?> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return await _unitOfWork.Scope.GetByNameAsync(name);
        }

        public async IAsyncEnumerable<Scope> FindByNamesAsync(ImmutableArray<string> names, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (names.IsDefaultOrEmpty)
                yield break;

            var all = await _unitOfWork.Scope.GetAllAsync();
            var filtered = all.Where(s => names.Contains(s.Name ?? string.Empty));

            foreach (var scope in filtered)
            {
                yield return scope;
            }
        }

        public async IAsyncEnumerable<Scope> FindByResourceAsync(string resource, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(resource))
                yield break;

            var all = await _unitOfWork.Scope.GetAllAsync();
            var filtered = all.Where(s => s.Resources != null && s.Resources.Contains(resource));

            foreach (var scope in filtered)
            {
                yield return scope;
            }
        }

        public ValueTask<string?> GetDescriptionAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new ValueTask<string?>(scope.Description);
        }

        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var dict = ImmutableDictionary.CreateBuilder<CultureInfo, string>();
            if (!string.IsNullOrEmpty(scope.Description))
            {
                dict.Add(CultureInfo.InvariantCulture, scope.Description);
            }

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(dict.ToImmutable());
        }

        public ValueTask<string?> GetDisplayNameAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new ValueTask<string?>(scope.DisplayName);
        }

        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var dict = ImmutableDictionary.CreateBuilder<CultureInfo, string>();
            if (!string.IsNullOrEmpty(scope.DisplayName))
            {
                dict.Add(CultureInfo.InvariantCulture, scope.DisplayName);
            }

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(dict.ToImmutable());
        }

        public ValueTask<string?> GetIdAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new ValueTask<string?>(scope.Id.ToString());
        }

        public ValueTask<string?> GetNameAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new ValueTask<string?>(scope.Name);
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();
            if (scope.Properties != null)
            {
                foreach (var prop in scope.Properties)
                {
                    builder.Add(prop.Key, JsonSerializer.SerializeToElement(prop.Value));
                }
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(builder.ToImmutable());
        }

        public ValueTask<ImmutableArray<string>> GetResourcesAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new ValueTask<ImmutableArray<string>>(
                scope.Resources?.ToImmutableArray() ?? ImmutableArray<string>.Empty);
        }

        public ValueTask<Scope> InstantiateAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<Scope>(new Scope
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });
        }

        public async IAsyncEnumerable<Scope> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Scope.GetAllAsync();
            var query = all.AsEnumerable();

            if (offset.HasValue)
                query = query.Skip(offset.Value);

            if (count.HasValue)
                query = query.Take(count.Value);

            foreach (var scope in query)
            {
                yield return scope;
            }
        }

        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Scope>, TState, IQueryable<TResult>> query, TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }

        public ValueTask SetDescriptionAsync(Scope scope, string? description, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            scope.Description = description;
            return default;
        }

        public ValueTask SetDescriptionsAsync(Scope scope, ImmutableDictionary<CultureInfo, string> descriptions, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (descriptions.TryGetValue(CultureInfo.InvariantCulture, out var description))
            {
                scope.Description = description;
            }

            return default;
        }

        public ValueTask SetDisplayNameAsync(Scope scope, string? name, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            scope.DisplayName = name;
            return default;
        }

        public ValueTask SetDisplayNamesAsync(Scope scope, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (names.TryGetValue(CultureInfo.InvariantCulture, out var name))
            {
                scope.DisplayName = name;
            }

            return default;
        }

        public ValueTask SetNameAsync(Scope scope, string? name, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            scope.Name = name;
            return default;
        }

        public ValueTask SetPropertiesAsync(Scope scope, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            scope.Properties = properties.ToDictionary(
                p => p.Key,
                p => p.Value.GetString() ?? string.Empty);

            return default;
        }

        public ValueTask SetResourcesAsync(Scope scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            scope.Resources = resources.ToList();
            return default;
        }

        public async ValueTask UpdateAsync(Scope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            await _unitOfWork.Scope.UpdateAsync(scope);
        }

        /// <summary>
        /// GetAsync - 用于查询
        /// </summary>
        public ValueTask<TResult?> GetAsync<TState, TResult>(
            Func<IQueryable<Scope>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken)
        {
            // Dapper 不支持 IQueryable
            return new ValueTask<TResult?>(default(TResult));
        }
    }
}
