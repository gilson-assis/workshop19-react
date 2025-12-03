
using CampusWorkshops.Api.Dtos;
using CampusWorkshops.Api.Models;
using CampusWorkshops.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusWorkshops.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkshopsController : ControllerBase
{
    private readonly IWorkshopRepository _repo;

    public WorkshopsController(IWorkshopRepository repo) => _repo = repo;

    /// <summary>Lista workshops com filtros opcionais.</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] string? q, CancellationToken ct)
    {
        var workshops = await _repo.GetAllAsync(from, to, q, ct);

        var response = workshops.Select(w => new WorkshopResponse(
            w.Id,
            w.Title,
            w.Description,
            w.StartAt,
            w.EndAt,
            w.Location,
            w.Capacity,
            w.IsOnline
        )).ToList();

        return Ok(response);
    }

    /// <summary>Obtém um workshop por Id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var workshop = await _repo.GetByIdAsync(id, ct);

        if (workshop == null)
        {
            return NotFound();
        }

        var response = new WorkshopResponse(
            workshop.Id,
            workshop.Title,
            workshop.Description,
            workshop.StartAt,
            workshop.EndAt,
            workshop.Location,
            workshop.Capacity,
            workshop.IsOnline
        );

        return Ok(response);
    }

    /// <summary>Cria um novo workshop.</summary>
    [HttpPost]
    [Authorize(Policy = "CanWriteWorkshops")]
    public async Task<IActionResult> Create([FromBody] CreateWorkshopRequest body, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validações de negócio
        if (body.StartAt >= body.EndAt)
        {
            return BadRequest("Data de início deve ser anterior à data de fim.");
        }

        if (body.StartAt < DateTimeOffset.UtcNow)
        {
            return BadRequest("Data de início deve ser no futuro.");
        }

        if (!body.IsOnline && string.IsNullOrWhiteSpace(body.Location))
        {
            return BadRequest("Location é obrigatório para workshops presenciais.");
        }

        if (body.Capacity < 1)
        {
            return BadRequest("Capacidade deve ser pelo menos 1.");
        }

        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = body.Title,
            Description = body.Description,
            StartAt = body.StartAt,
            EndAt = body.EndAt,
            Location = body.Location,
            Capacity = body.Capacity,
            IsOnline = body.IsOnline
        };

        var createdWorkshop = await _repo.AddAsync(workshop, ct);

        var response = new WorkshopResponse(
            createdWorkshop.Id,
            createdWorkshop.Title,
            createdWorkshop.Description,
            createdWorkshop.StartAt,
            createdWorkshop.EndAt,
            createdWorkshop.Location,
            createdWorkshop.Capacity,
            createdWorkshop.IsOnline
        );

        return CreatedAtAction(nameof(GetById), new { id = createdWorkshop.Id }, response);
    }

    /// <summary>Atualiza parcialmente um workshop existente.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "CanWriteWorkshops")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchWorkshopRequest body, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingWorkshop = await _repo.GetByIdAsync(id, ct);
        if (existingWorkshop == null)
        {
            return NotFound();
        }

        // Aplicar apenas os campos fornecidos
        if (body.Title != null)
            existingWorkshop.Title = body.Title;

        if (body.Description != null)
            existingWorkshop.Description = body.Description;

        if (body.StartAt.HasValue)
        {
            if (body.StartAt.Value < DateTimeOffset.UtcNow)
            {
                return BadRequest("Data de início deve ser no futuro.");
            }
            existingWorkshop.StartAt = body.StartAt.Value;
        }

        if (body.EndAt.HasValue)
            existingWorkshop.EndAt = body.EndAt.Value;

        if (body.Location != null)
            existingWorkshop.Location = body.Location;

        if (body.Capacity.HasValue)
        {
            if (body.Capacity.Value < 1)
            {
                return BadRequest("Capacidade deve ser pelo menos 1.");
            }
            existingWorkshop.Capacity = body.Capacity.Value;
        }

        if (body.IsOnline.HasValue)
            existingWorkshop.IsOnline = body.IsOnline.Value;

        // Validações de negócio após aplicar mudanças
        if (existingWorkshop.StartAt >= existingWorkshop.EndAt)
        {
            return BadRequest("Data de início deve ser anterior à data de fim.");
        }

        if (!existingWorkshop.IsOnline && string.IsNullOrWhiteSpace(existingWorkshop.Location))
        {
            return BadRequest("Location é obrigatório para workshops presenciais.");
        }

        var updatedWorkshop = await _repo.UpdateAsync(existingWorkshop, ct);
        if (updatedWorkshop == null)
        {
            return NotFound();
        }

        var response = new WorkshopResponse(
            updatedWorkshop.Id,
            updatedWorkshop.Title,
            updatedWorkshop.Description,
            updatedWorkshop.StartAt,
            updatedWorkshop.EndAt,
            updatedWorkshop.Location,
            updatedWorkshop.Capacity,
            updatedWorkshop.IsOnline
        );

        return Ok(response);
    }

    /// <summary>Remove um workshop.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDeleteWorkshops")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var workshop = await _repo.GetByIdAsync(id, ct);
        if (workshop == null)
        {
            return NotFound();
        }

        await _repo.DeleteAsync(id, ct);
        return NoContent();
    }
}
