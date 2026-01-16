namespace ClinicManagement_API.Features.auth_service.dto;

public record RegisterDto(
    string Username,
    string Password,
    // Optional patient info - if provided, will create Patient record
    string? FullName = null,
    string? Phone = null,
    string? Email = null,
    Guid? ClinicId = null // Required for creating patient
);

public record LoginDto(string Username, string Password);

// Response DTOs for standardized format
public record RegisterResponse(Guid UserId, Guid? PatientId);

public record LoginResponse(Guid Id, string AccessToken);