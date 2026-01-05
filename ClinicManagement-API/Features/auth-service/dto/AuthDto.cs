namespace ClinicManagement_API.Features.auth_service.dto;

public record RegisterDto(string Username, string Password);
public record LoginDto(string Username, string Password);