namespace ClinicManagement_API.Features.auth_service.dto;

public record RegisterDto(string Username, string Password);

public record LoginDto(string Username, string Password);

// Response DTOs for standardized format
public record RegisterResponse(Guid UserId);

public record LoginResponse(Guid Id, string AccessToken);