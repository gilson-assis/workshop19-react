
namespace CampusWorkshops.Api.Dtos;

// DTO de resposta para Workshop
public record WorkshopResponse(
	Guid Id,
	string? Title,
	string? Description,
	DateTimeOffset StartAt,
	DateTimeOffset EndAt,
	string? Location,
	int Capacity,
	bool IsOnline
);
