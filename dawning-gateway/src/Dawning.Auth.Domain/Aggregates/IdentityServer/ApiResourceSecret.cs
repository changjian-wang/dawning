namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public class ApiResourceSecret : Secret
{
    public Guid ApiResourceId { get; set; }
}