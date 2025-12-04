using Dawning.Identity.Domain.Aggregates.OpenIddict;
using Dawning.Identity.Domain.Interfaces.UoW;
using OpenIddict.Abstractions;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Dawning.Identity.Infra.Data.Stores
{
    /// <summary>
    /// OpenIddict Token Store - 桥接到 Dapper Repository
    /// </summary>
    public class OpenIddictTokenStore : IOpenIddictTokenStore<Token>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OpenIddictTokenStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            return all.Count();
        }

        public ValueTask<long> CountAsync<TResult>(Func<IQueryable<Token>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            return CountAsync(cancellationToken);
        }

        public async ValueTask CreateAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (token.Id == Guid.Empty)
                token.Id = Guid.NewGuid();

            token.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Token.InsertAsync(token);
        }

        public async ValueTask DeleteAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            await _unitOfWork.Token.DeleteAsync(token);
        }

        public async IAsyncEnumerable<Token> FindAsync(string subject, string client, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t =>
                t.Subject == subject &&
                t.ApplicationId.ToString() == client);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<Token> FindAsync(string subject, string client, string status, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t =>
                t.Subject == subject &&
                t.ApplicationId.ToString() == client &&
                t.Status == status);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<Token> FindAsync(string subject, string client, string status, string type, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t =>
                t.Subject == subject &&
                t.ApplicationId.ToString() == client &&
                t.Status == status &&
                t.Type == type);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<Token> FindByApplicationIdAsync(string identifier, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(identifier, out var appId))
                yield break;

            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t => t.ApplicationId == appId);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<Token> FindByAuthorizationIdAsync(string identifier, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(identifier, out var authId))
                yield break;

            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t => t.AuthorizationId == authId);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public async ValueTask<Token?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            if (!Guid.TryParse(identifier, out var guid))
                return null;

            var token = await _unitOfWork.Token.GetAsync(guid);
            return token.Id == Guid.Empty ? null : token;
        }

        public async ValueTask<Token?> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            var all = await _unitOfWork.Token.GetAllAsync();
            return all.FirstOrDefault(t => t.ReferenceId == identifier);
        }

        public async IAsyncEnumerable<Token> FindBySubjectAsync(string subject, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
                yield break;

            var all = await _unitOfWork.Token.GetAllAsync();
            var filtered = all.Where(t => t.Subject == subject);

            foreach (var token in filtered)
            {
                yield return token;
            }
        }

        public ValueTask<string?> GetApplicationIdAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.ApplicationId?.ToString());
        }

        public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<Token>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            return new ValueTask<TResult?>(default(TResult));
        }

        public ValueTask<string?> GetAuthorizationIdAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.AuthorizationId?.ToString());
        }

        public ValueTask<DateTimeOffset?> GetCreationDateAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<DateTimeOffset?>(new DateTimeOffset(token.CreatedAt, TimeSpan.Zero));
        }

        public ValueTask<DateTimeOffset?> GetExpirationDateAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<DateTimeOffset?>(token.ExpirationDate.HasValue 
                ? new DateTimeOffset(token.ExpirationDate.Value, TimeSpan.Zero) 
                : null);
        }

        public ValueTask<string?> GetIdAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.Id.ToString());
        }

        public ValueTask<string?> GetPayloadAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.Payload);
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();
            if (token.Properties != null)
            {
                foreach (var prop in token.Properties)
                {
                    builder.Add(prop.Key, JsonSerializer.SerializeToElement(prop.Value));
                }
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(builder.ToImmutable());
        }

        public ValueTask<DateTimeOffset?> GetRedemptionDateAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<DateTimeOffset?>(token.RedemptionDate.HasValue
                ? new DateTimeOffset(token.RedemptionDate.Value, TimeSpan.Zero)
                : null);
        }

        public ValueTask<string?> GetReferenceIdAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.ReferenceId);
        }

        public ValueTask<string?> GetStatusAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.Status);
        }

        public ValueTask<string?> GetSubjectAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.Subject);
        }

        public ValueTask<string?> GetTypeAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return new ValueTask<string?>(token.Type);
        }

        public ValueTask<Token> InstantiateAsync(CancellationToken cancellationToken)
        {
            return new ValueTask<Token>(new Token
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            });
        }

        public async IAsyncEnumerable<Token> ListAsync(int? count, int? offset, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            var query = all.AsEnumerable();

            if (offset.HasValue)
                query = query.Skip(offset.Value);

            if (count.HasValue)
                query = query.Take(count.Value);

            foreach (var token in query)
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Token>, TState, IQueryable<TResult>> query, TState state, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }

        public async ValueTask<long> PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
        {
            var all = await _unitOfWork.Token.GetAllAsync();
            var toDelete = all.Where(t => 
                t.Status != OpenIddictConstants.Statuses.Valid &&
                (t.ExpirationDate.HasValue && t.ExpirationDate.Value < threshold.UtcDateTime)).ToList();

            foreach (var token in toDelete)
            {
                await _unitOfWork.Token.DeleteAsync(token);
            }

            return toDelete.Count;
        }

        /// <summary>
        /// 根据 Authorization ID 撤销令牌
        /// </summary>
        public async ValueTask<long> RevokeByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier) || !Guid.TryParse(identifier, out var authId))
                return 0;

            var all = await _unitOfWork.Token.GetAllAsync();
            var tokens = all.Where(t => t.AuthorizationId == authId).ToList();

            foreach (var token in tokens)
            {
                token.Status = OpenIddictConstants.Statuses.Revoked;
                await _unitOfWork.Token.UpdateAsync(token);
            }

            return tokens.Count;
        }

        public ValueTask SetApplicationIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.ApplicationId = string.IsNullOrEmpty(identifier) ? null : Guid.Parse(identifier);
            return default;
        }

        public ValueTask SetAuthorizationIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.AuthorizationId = string.IsNullOrEmpty(identifier) ? null : Guid.Parse(identifier);
            return default;
        }

        public ValueTask SetCreationDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.CreatedAt = date?.UtcDateTime ?? DateTime.UtcNow;
            return default;
        }

        public ValueTask SetExpirationDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.ExpirationDate = date?.UtcDateTime;
            return default;
        }

        public ValueTask SetPayloadAsync(Token token, string? payload, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Payload = payload;
            return default;
        }

        public ValueTask SetPropertiesAsync(Token token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Properties = properties.ToDictionary(
                p => p.Key,
                p => p.Value.GetString() ?? string.Empty);

            return default;
        }

        public ValueTask SetRedemptionDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.RedemptionDate = date?.UtcDateTime;
            return default;
        }

        public ValueTask SetReferenceIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.ReferenceId = identifier;
            return default;
        }

        public ValueTask SetStatusAsync(Token token, string? status, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Status = status;
            return default;
        }

        public ValueTask SetSubjectAsync(Token token, string? subject, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Subject = subject;
            return default;
        }

        public ValueTask SetTypeAsync(Token token, string? type, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            token.Type = type;
            return default;
        }

        public async ValueTask UpdateAsync(Token token, CancellationToken cancellationToken)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            await _unitOfWork.Token.UpdateAsync(token);
        }
    }
}
