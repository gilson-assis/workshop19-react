
using System.ComponentModel.DataAnnotations;

namespace CampusWorkshops.Api.Dtos;

// DTO para criação de Workshop com validações
public record CreateWorkshopRequest(
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 120 caracteres")]
    string Title,
    
    [StringLength(2000, ErrorMessage = "Descrição não pode exceder 2000 caracteres")]
    string? Description,
    
    [Required(ErrorMessage = "Data de início é obrigatória")]
    DateTimeOffset StartAt,
    
    [Required(ErrorMessage = "Data de fim é obrigatória")]
    DateTimeOffset EndAt,
    
    [StringLength(200, ErrorMessage = "Localização não pode exceder 200 caracteres")]
    string? Location,
    
    [Range(1, 1000, ErrorMessage = "Capacidade deve estar entre 1 e 1000")]
    int Capacity,
    
    bool IsOnline
);
