public record CreateClinicRequest(
    
    string Code,
    string Name,
    string? Phone,
    string? Email);

public record UpdateClinicRequest(
    string Code,
    string Name,
    string? Phone,
    string? Email);
