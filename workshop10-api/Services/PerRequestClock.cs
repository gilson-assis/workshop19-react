namespace CampusWorkshops.Api.Services;

public interface IPerRequestClock
{
    // Carimbo criado no CONSTRUTOR (não muda depois)
    DateTimeOffset CreatedAt { get; }
}

public sealed class PerRequestClock : IPerRequestClock
{
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}
