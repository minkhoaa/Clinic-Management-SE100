namespace ClinicManagement_API.Domains.Enums;

/// <summary>
/// Role name constants for authorization.
/// Used with ASP.NET Identity - stored as strings in database.
/// </summary>
public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Receptionist = "RECEPTIONIST";
    public const string Doctor = "DOCTOR";
    public const string User = "USER";
}