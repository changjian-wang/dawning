namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public class ApiResourceClaim : UserClaim
{
    public Guid ApiResourceId { get; set; }
}