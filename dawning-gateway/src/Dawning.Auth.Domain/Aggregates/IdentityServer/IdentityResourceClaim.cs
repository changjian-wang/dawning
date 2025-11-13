namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public class IdentityResourceClaim : UserClaim
{
    public Guid IdentityResourceId { get; set; }
}