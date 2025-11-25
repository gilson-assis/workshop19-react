namespace CampusWorkshops.Api.Services;

public interface IRequestIdTransient { Guid Id { get; } }
public interface IRequestIdScoped    { Guid Id { get; } }
public interface IRequestIdSingleton { Guid Id { get; } }

internal sealed class RequestIdTransient : IRequestIdTransient { public Guid Id { get; } = Guid.NewGuid(); }
internal sealed class RequestIdScoped    : IRequestIdScoped    { public Guid Id { get; } = Guid.NewGuid(); }
internal sealed class RequestIdSingleton : IRequestIdSingleton { public Guid Id { get; } = Guid.NewGuid(); }
