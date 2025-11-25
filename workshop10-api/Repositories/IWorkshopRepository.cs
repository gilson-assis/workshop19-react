
using CampusWorkshops.Api.Models;

namespace CampusWorkshops.Api.Repositories;

// Define repository contract; implementations should be provided by students during the workshop.
public interface IWorkshopRepository
{
    Task<IReadOnlyList<Workshop>> GetAllAsync(DateTimeOffset? from, DateTimeOffset? to, string? q, CancellationToken ct);
    Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Workshop> AddAsync(Workshop workshop, CancellationToken ct);
    Task<Workshop?> UpdateAsync(Workshop workshop, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<Workshop?> UpdatePartialAsync(Guid id, Action<Workshop> updateAction, CancellationToken ct);
}
