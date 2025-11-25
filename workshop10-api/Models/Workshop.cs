
using System.ComponentModel.DataAnnotations;

namespace CampusWorkshops.Api.Models;

// Modelo mínimo para Workshop com validações
public class Workshop
{
    // Identificador único
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 120 caracteres")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "Descrição não pode exceder 2000 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTimeOffset StartAt { get; set; }

    [Required(ErrorMessage = "Data de fim é obrigatória")]
    public DateTimeOffset EndAt { get; set; }

    // Location deve ser obrigatório se IsOnline == false (validado no controller)
    [StringLength(200, ErrorMessage = "Localização não pode exceder 200 caracteres")]
    public string? Location { get; set; }

    [Range(1, 1000, ErrorMessage = "Capacidade deve estar entre 1 e 1000")]
    public int Capacity { get; set; } = 1;

    public bool IsOnline { get; set; }
}
