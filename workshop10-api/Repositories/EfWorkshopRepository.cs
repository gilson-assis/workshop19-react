using CampusWorkshops.Api.Models;
using Microsoft.EntityFrameworkCore;
using CampusWorkshops.Api.Infrastructure.Data;

namespace CampusWorkshops.Api.Repositories;

public class EfWorkshopRepository : IWorkshopRepository
{
    private readonly WorkshopsDbContext _db;
    public EfWorkshopRepository(WorkshopsDbContext db) => _db = db;

    public async Task<IReadOnlyList<Workshop>> GetAllAsync(DateTimeOffset? from, DateTimeOffset? to, string? q, CancellationToken ct)
    {
        var query = _db.Workshops.AsNoTracking().AsQueryable();
        if (from.HasValue)
        {
            query = query.Where(w => w.StartAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(w => w.EndAt >= to.Value);
        }

 
        if (!string.IsNullOrWhiteSpace(q))
        {
            var termo = q.Trim();
            query = query.Where(w =>
                (w.Title != null && w.Title.Contains(termo)) ||
                (w.Description != null && w.Description.Contains(termo))
            );
        }

        Console.WriteLine(query.ToQueryString());
        return await query.ToListAsync(ct);
        
    }

    public async Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Workshops.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<Workshop> AddAsync(Workshop workshop, CancellationToken ct)
    {
        _db.Workshops.Add(workshop);
        await _db.SaveChangesAsync();
        return workshop;
    }

    public async Task<Workshop?> UpdateAsync(Workshop workshop, CancellationToken ct)
    {
        _db.Workshops.Update(workshop);
        await _db.SaveChangesAsync(ct);
        return workshop;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var encontrado = await _db.Workshops.FindAsync([id]);
        if (encontrado == null)
        {
            return false;
        }
        _db.Workshops.Remove(encontrado);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => _db.Workshops.AnyAsync(w => w.Id == id, ct);

    public async Task<Workshop?> UpdatePartialAsync(Guid id, Action<Workshop> updateAction, CancellationToken ct)
    {
        var w = await _db.Workshops.FirstOrDefaultAsync(x => x.Id == id, ct);
        // TODO: implementar o UpdatePartial
        return w;
    }
}
