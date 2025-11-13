namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public abstract class UserClaim
{
    public Guid Id { get; set; }

    public string? Type { get; set; }
}