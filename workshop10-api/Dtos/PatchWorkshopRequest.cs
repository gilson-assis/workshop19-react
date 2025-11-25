using System.ComponentModel.DataAnnotations;

namespace CampusWorkshops.Api.Dtos;

// DTO para atualização parcial de Workshop.
public record PatchWorkshopRequest(
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 120 caracteres")]
    string? Title,
    
    [StringLength(2000, ErrorMessage = "Descrição não pode exceder 2000 caracteres")]
    string? Description,
    
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt,
    
    [StringLength(200, ErrorMessage = "Localização não pode exceder 200 caracteres")]
    string? Location,
    
    [Range(1, 1000, ErrorMessage = "Capacidade deve estar entre 1 e 1000")]
    int? Capacity,
    
    bool? IsOnline
);
