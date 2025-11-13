namespace Dawning.Auth.Domain.Aggregates.IdentityServer;

public abstract class Property
{
    public Guid Id { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }
}