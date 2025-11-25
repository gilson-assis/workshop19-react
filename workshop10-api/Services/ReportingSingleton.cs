using System;

namespace CampusWorkshops.Api.Services;

/// <summary>
/// Singleton que recebe um Transient (IPerRequestClock).
/// Na prática, o Transient é resolvido uma única vez, na construção do Singleton,
/// e "vira" efetivamente singleton dentro dele.
/// </summary>
public sealed class ReportingSingleton
{
    private readonly IPerRequestClock _clock;

    public ReportingSingleton(IPerRequestClock clock)
    {
        _clock = clock;
    }

    public DateTimeOffset GetClockCreatedAt()
    {
        return _clock.CreatedAt;
    }
}
