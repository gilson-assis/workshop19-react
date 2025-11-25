using CampusWorkshops.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CampusWorkshops.Api.Repositories;

public sealed class CachedWorkshopRepository : IWorkshopRepository
{
    private readonly IWorkshopRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;

    public CachedWorkshopRepository(
        IWorkshopRepository inner,
        IMemoryCache cache,
        IOptionsSnapshot<CampusWorkshops.Api.Services.CacheSettings> opts)
    {
        _inner = inner;
        _cache = cache;
        _ttl = TimeSpan.FromSeconds(opts.Value.DefaultTtlSeconds);
    }

    private static string KeyById(Guid id) => $"workshops:byId:{id}";
    private static string KeyList(DateTimeOffset? from, DateTimeOffset? to, string? q)
        => $"workshops:list:{from?.ToUnixTimeSeconds()}:{to?.ToUnixTimeSeconds()}:{q?.Trim().ToLowerInvariant()}";
    public async Task<IReadOnlyList<Workshop>> GetAllAsync(
        DateTimeOffset? from, DateTimeOffset? to, string? q, CancellationToken ct)
    {   
        // Implementação final
        var key = KeyList(from, to, q);
        if (_cache.TryGetValue(key, out IReadOnlyList<Workshop>? cached) && cached is not null)
            return cached;

        var data = await _inner.GetAllAsync(from, to, q, ct);
        _cache.Set(key, data, _ttl);
        return data;
    }


    public async Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var key = KeyById(id);
        if (_cache.TryGetValue(key, out Workshop? cached) && cached is not null)
            return cached;

        var w = await _inner.GetByIdAsync(id, ct);
        if (w is not null) _cache.Set(key, w, _ttl);
        return w;
    }

    // Ações de escrita: apenas encaminham e limpam entradas simples
    public async Task<Workshop> AddAsync(Workshop workshop, CancellationToken ct)
    {
        var w = await _inner.AddAsync(workshop, ct);
        _cache.Remove(KeyById(w.Id));
        return w;
    }

    public async Task<Workshop?> UpdateAsync(Workshop workshop, CancellationToken ct)
    {
        var w = await _inner.UpdateAsync(workshop, ct);
        if (w is not null) _cache.Remove(KeyById(w.Id));
        return w;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var ok = await _inner.DeleteAsync(id, ct);
        if (ok) _cache.Remove(KeyById(id));
        return ok;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => _inner.ExistsAsync(id, ct);

    public async Task<Workshop?> UpdatePartialAsync(Guid id, Action<Workshop> updateAction, CancellationToken ct)
    {
        var w = await _inner.UpdatePartialAsync(id, updateAction, ct);
        if (w is not null) _cache.Remove(KeyById(w.Id));
        return w;
    }
}
