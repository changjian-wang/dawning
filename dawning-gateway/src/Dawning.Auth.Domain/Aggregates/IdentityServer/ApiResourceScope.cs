namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public class ApiResourceScope
{
    public Guid Id { get; set; }

    public string? Scope { get; set; }

    public Guid ApiResourceId { get; set; }
}