
using CampusWorkshops.Api.Models;

namespace CampusWorkshops.Api.Repositories;

// In-memory repository stub
public class InMemoryWorkshopRepository : IWorkshopRepository
{
    List<Workshop> _workshops = new();

    private void Seed()
    {
        // Base de datas: sempre a partir da próxima semana, para não “envelhecer” o seed
        var baseDate = DateTimeOffset.UtcNow.Date.AddDays(7);

        var w1 = new Workshop
        {
            Id = Guid.Parse("8a0b9f1b-6c9b-4a2a-9d5e-1c2f3a4b5c6d"),
            Title = "APIs RESTful — visão geral",
            Description = "Modelagem de recursos, status codes e boas práticas.",
            StartAt = baseDate.AddHours(12),   // daqui a 7 dias, 12:00 UTC
            EndAt   = baseDate.AddHours(15),   // 15:00 UTC
            Location = "Lab 101",
            Capacity = 25,
            IsOnline = false
        };

        var w2 = new Workshop
        {
            Id = Guid.Parse("1c3d5e7f-9a2b-4c6d-8e0f-1a2b3c4d5e6f"),
            Title = "Entity Framework — mapeamentos e LINQ",
            Description = "Relações, consultas eficientes e boas práticas.",
            StartAt = baseDate.AddDays(2).AddHours(12), // +2 dias, 12:00 UTC
            EndAt   = baseDate.AddDays(2).AddHours(14), // 14:00 UTC
            Location = null,   // online
            Capacity = 40,
            IsOnline = true
        };

        var w3 = new Workshop
        {
            Id = Guid.Parse("f0e1d2c3-b4a5-6789-9a8b-7c6d5e4f3a2b"),
            Title = "Construindo contratos com Swagger/OpenAPI",
            Description = "Documentação viva, exemplos e testes via Swagger UI.",
            StartAt = baseDate.AddDays(5).AddHours(13), // +5 dias, 13:00 UTC
            EndAt   = baseDate.AddDays(5).AddHours(16), // 16:00 UTC
            Location = "Auditório Central",
            Capacity = 80,
            IsOnline = false
        };

        _workshops.Add(w1);
        _workshops.Add(w2);
        _workshops.Add(w3);
    }


    public InMemoryWorkshopRepository()
    {
        Seed();
    }

    public Task<IReadOnlyList<Workshop>> GetAllAsync(DateTimeOffset? from, DateTimeOffset? to, string? q, CancellationToken ct)
    {
        var query = _workshops.AsQueryable();

        // Filtro por data início (from)
        if (from.HasValue)
        {
            query = query.Where(w => w.StartAt >= from.Value);
        }

        // Filtro por data fim (to)  
        if (to.HasValue)
        {
            query = query.Where(w => w.StartAt <= to.Value);
        }

        // Filtro por texto (q) - busca no título e descrição
        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchTerm = q.Trim().ToLowerInvariant();
            query = query.Where(w => 
                (w.Title != null && w.Title.ToLowerInvariant().Contains(searchTerm)) ||
                (w.Description != null && w.Description.ToLowerInvariant().Contains(searchTerm))
            );
        }

        // Ordenar por data de início
        var result = query.OrderBy(w => w.StartAt).ToList();
        
        return Task.FromResult<IReadOnlyList<Workshop>>(result);
    }

    public Task<Workshop?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var workshop = _workshops.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(workshop);
    }

    public Task<Workshop> AddAsync(Workshop workshop, CancellationToken ct)
    {
        // Gerar novo ID se não foi fornecido ou se já existe
        if (workshop.Id == Guid.Empty || _workshops.Any(w => w.Id == workshop.Id))
        {
            workshop.Id = Guid.NewGuid();
        }

        // Validações de negócio que serão úteis quando migrar para banco
        ValidateWorkshop(workshop);

        _workshops.Add(workshop);
        return Task.FromResult(workshop);
    }

    public Task<Workshop?> UpdateAsync(Workshop workshop, CancellationToken ct)
    {
        var existingIndex = _workshops.FindIndex(w => w.Id == workshop.Id);
        if (existingIndex < 0)
        {
            return Task.FromResult<Workshop?>(null);
        }

        // Validações de negócio
        ValidateWorkshop(workshop);

        _workshops[existingIndex] = workshop;
        return Task.FromResult<Workshop?>(workshop);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var workshop = _workshops.FirstOrDefault(w => w.Id == id);
        if (workshop == null)
        {
            return Task.FromResult(false);
        }

        _workshops.Remove(workshop);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        var exists = _workshops.Any(w => w.Id == id);
        return Task.FromResult(exists);
    }

    public Task<Workshop?> UpdatePartialAsync(Guid id, Action<Workshop> updateAction, CancellationToken ct)
    {
        var workshop = _workshops.FirstOrDefault(w => w.Id == id);
        if (workshop == null)
        {
            return Task.FromResult<Workshop?>(null);
        }

        // Aplicar as modificações
        updateAction(workshop);

        // Validar após modificações
        ValidateWorkshop(workshop);

        return Task.FromResult<Workshop?>(workshop);
    }

    private void ValidateWorkshop(Workshop workshop)
    {
        var errors = new List<string>();

        // Validação de título
        if (string.IsNullOrWhiteSpace(workshop.Title))
        {
            errors.Add("Title is required");
        }
        else if (workshop.Title.Length < 3 || workshop.Title.Length > 120)
        {
            errors.Add("Title must be between 3 and 120 characters");
        }

        // Validação de descrição
        if (!string.IsNullOrWhiteSpace(workshop.Description) && workshop.Description.Length > 2000)
        {
            errors.Add("Description cannot exceed 2000 characters");
        }

        // Validação de datas
        if (workshop.EndAt <= workshop.StartAt)
        {
            errors.Add("End date must be after start date");
        }

        if (workshop.StartAt <= DateTimeOffset.UtcNow)
        {
            errors.Add("Start date must be in the future");
        }

        // Validação de capacidade
        if (workshop.Capacity < 1)
        {
            errors.Add("Capacity must be at least 1");
        }

        // Validação de localização para workshops presenciais
        if (!workshop.IsOnline && string.IsNullOrWhiteSpace(workshop.Location))
        {
            errors.Add("Location is required for in-person workshops");
        }

        // Validação de localização para workshops online
        if (workshop.IsOnline && !string.IsNullOrWhiteSpace(workshop.Location))
        {
            errors.Add("Location should not be specified for online workshops");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Workshop validation failed: {string.Join(", ", errors)}");
        }
    }
}
