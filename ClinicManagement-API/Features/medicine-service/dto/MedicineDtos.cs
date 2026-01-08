namespace ClinicManagement_API.Features.medicine_service.dto;

// Medicine CRUD DTOs
public record MedicineListDto(
    Guid MedicineId,
    string Code,
    string Name,
    string? Unit,
    decimal? Price,
    bool IsActive
);

public record CreateMedicineRequest(
    string Code,
    string Name,
    string? Unit,
    decimal? Price,
    string? Description
);

public record UpdateMedicineRequest(
    string? Name,
    string? Unit,
    decimal? Price,
    string? Description,
    bool? IsActive
);

public record CreateMedicineResponse(
    Guid MedicineId,
    DateTime CreatedAt
);
