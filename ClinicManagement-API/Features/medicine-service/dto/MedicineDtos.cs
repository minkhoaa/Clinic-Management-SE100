namespace ClinicManagement_API.Features.medicine_service.dto;

// Medicine CRUD DTOs
public record MedicineListDto(
    Guid MedicineId,
    string Code,
    string Name,
    string? Unit,
    decimal? Price,
    int StockQuantity,
    int MinStockLevel,
    DateTime? ExpiryDate,
    bool IsActive,
    bool IsLowStock // StockQuantity <= MinStockLevel
);

public record CreateMedicineRequest(
    string Code,
    string Name,
    string? Unit,
    decimal? Price,
    string? Description,
    int? StockQuantity,
    int? MinStockLevel,
    DateTime? ExpiryDate
);

public record UpdateMedicineRequest(
    string? Name,
    string? Unit,
    decimal? Price,
    string? Description,
    bool? IsActive,
    int? StockQuantity,
    int? MinStockLevel,
    DateTime? ExpiryDate
);

public record CreateMedicineResponse(
    Guid MedicineId,
    DateTime CreatedAt
);

// Low Stock Warning DTO
public record LowStockMedicineDto(
    Guid MedicineId,
    string Code,
    string Name,
    string? Unit,
    int StockQuantity,
    int MinStockLevel,
    DateTime? ExpiryDate,
    bool IsExpiringSoon // ExpiryDate < 30 days
);

public record UpdateStockRequest(
    int Quantity,
    string? Notes // Lý do: Nhập kho, Kiểm kê...
);
