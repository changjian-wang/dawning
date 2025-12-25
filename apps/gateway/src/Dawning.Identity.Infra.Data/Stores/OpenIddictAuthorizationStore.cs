using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using OpenIddict.Abstractions;

namespace Dawning.Identity.Infra.Data.Stores
{
    /// <summary>
    /// OpenIddict Authorization Store - 桥接到 Dapper Repository
    /// </summary>
    public class OpenIddictAuthorizationStore : IOpenIddictAuthorizationStore<Authorization>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenIddictAuthorizationStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            return all.Count();
        }

        public ValueTask<long> CountAsync<TResult>(
            Func<IQueryable<Authorization>, IQueryable<TResult>> query,
            CancellationToken cancellationToken
        )
        {
            return CountAsync(cancellationToken);
        }

        public async ValueTask CreateAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            if (authorization.Id == Guid.Empty)
                authorization.Id = Guid.NewGuid();

            authorization.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Authorization.InsertAsync(authorization);
        }

        public async ValueTask DeleteAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            await _unitOfWork.Authorization.DeleteAsync(authorization);
        }

        public async IAsyncEnumerable<Authorization> FindAsync(
            string subject,
            string client,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a =>
                a.Subject == subject && a.ApplicationId.ToString() == client
            );

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public async IAsyncEnumerable<Authorization> FindAsync(
            string subject,
            string client,
            string status,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a =>
                a.Subject == subject && a.ApplicationId.ToString() == client && a.Status == status
            );

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public async IAsyncEnumerable<Authorization> FindAsync(
            string subject,
            string client,
            string status,
            string type,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a =>
                a.Subject == subject
                && a.ApplicationId.ToString() == client
                && a.Status == status
                && a.Type == type
            );

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public async IAsyncEnumerable<Authorization> FindAsync(
            string subject,
            string client,
            string status,
            string type,
            ImmutableArray<string> scopes,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a =>
                a.Subject == subject
                && a.ApplicationId.ToString() == client
                && a.Status == status
                && a.Type == type
                && (a.Scopes != null && scopes.All(s => a.Scopes.Contains(s)))
            );

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public async IAsyncEnumerable<Authorization> FindByApplicationIdAsync(
            string identifier,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            if (!Guid.TryParse(identifier, out var appId))
                yield break;

            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a => a.ApplicationId == appId);

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public async ValueTask<Authorization?> FindByIdAsync(
            string identifier,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            if (!Guid.TryParse(identifier, out var guid))
                return null;

            var auth = await _unitOfWork.Authorization.GetAsync(guid);
            return auth.Id == Guid.Empty ? null : auth;
        }

        public async IAsyncEnumerable<Authorization> FindBySubjectAsync(
            string subject,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrEmpty(subject))
                yield break;

            var all = await _unitOfWork.Authorization.GetAllAsync();
            var filtered = all.Where(a => a.Subject == subject);

            foreach (var auth in filtered)
            {
                yield return auth;
            }
        }

        public ValueTask<string?> GetApplicationIdAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<string?>(authorization.ApplicationId?.ToString());
        }

        public ValueTask<TResult?> GetAsync<TState, TResult>(
            Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query,
            TState state,
            CancellationToken cancellationToken
        )
        {
            return new ValueTask<TResult?>(default(TResult));
        }

        public ValueTask<DateTimeOffset?> GetCreationDateAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<DateTimeOffset?>(
                new DateTimeOffset(authorization.CreatedAt, TimeSpan.Zero)
            );
        }

        public ValueTask<string?> GetIdAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<string?>(authorization.Id.ToString());
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();
            if (authorization.Properties != null)
            {
                foreach (var prop in authorization.Properties)
                {
                    builder.Add(prop.Key, JsonSerializer.SerializeToElement(prop.Value));
                }
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(builder.ToImmutable());
        }

        public ValueTask<ImmutableArray<string>> GetScopesAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<ImmutableArray<string>>(
                authorization.Scopes?.ToImmutableArray() ?? ImmutableArray<string>.Empty
            );
        }

        public ValueTask<string?> GetStatusAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<string?>(authorization.Status);
        }

        public ValueTask<string?> GetSubjectAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<string?>(authorization.Subject);
        }

        public ValueTask<string?> GetTypeAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            return new ValueTask<string?>(authorization.Type);
        }

        public ValueTask<Authorization> InstantiateAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<Authorization>(
                new Authorization { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
        }

        public async IAsyncEnumerable<Authorization> ListAsync(
            int? count,
            int? offset,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var query = all.AsEnumerable();

            if (offset.HasValue)
                query = query.Skip(offset.Value);

            if (count.HasValue)
                query = query.Take(count.Value);

            foreach (var auth in query)
            {
                yield return auth;
            }
        }

        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query,
            TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            yield break;
        }

        public async ValueTask<long> PruneAsync(
            DateTimeOffset threshold,
            CancellationToken cancellationToken
        )
        {
            var all = await _unitOfWork.Authorization.GetAllAsync();
            var toDelete = all.Where(a => a.CreatedAt < threshold.UtcDateTime).ToList();

            foreach (var auth in toDelete)
            {
                await _unitOfWork.Authorization.DeleteAsync(auth);
            }

            return toDelete.Count;
        }

        public ValueTask SetApplicationIdAsync(
            Authorization authorization,
            string? identifier,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.ApplicationId = string.IsNullOrEmpty(identifier)
                ? null
                : Guid.Parse(identifier);
            return default;
        }

        public ValueTask SetCreationDateAsync(
            Authorization authorization,
            DateTimeOffset? date,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.CreatedAt = date?.UtcDateTime ?? DateTime.UtcNow;
            return default;
        }

        public ValueTask SetPropertiesAsync(
            Authorization authorization,
            ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.Properties = properties.ToDictionary(
                p => p.Key,
                p => p.Value.GetString() ?? string.Empty
            );

            return default;
        }

        public ValueTask SetScopesAsync(
            Authorization authorization,
            ImmutableArray<string> scopes,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.Scopes = scopes.ToList();
            return default;
        }

        public ValueTask SetStatusAsync(
            Authorization authorization,
            string? status,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.Status = status;
            return default;
        }

        public ValueTask SetSubjectAsync(
            Authorization authorization,
            string? subject,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.Subject = subject;
            return default;
        }

        public ValueTask SetTypeAsync(
            Authorization authorization,
            string? type,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            authorization.Type = type;
            return default;
        }

        public async ValueTask UpdateAsync(
            Authorization authorization,
            CancellationToken cancellationToken
        )
        {
            if (authorization == null)
                throw new ArgumentNullException(nameof(authorization));

            await _unitOfWork.Authorization.UpdateAsync(authorization);
        }
    }
}
