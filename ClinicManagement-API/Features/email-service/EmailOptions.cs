namespace ClinicManagement_API.Features.email_service;

public class EmailOptions
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromName { get; set; } = "Clinic Management";
    public string FromEmail { get; set; } = default!;
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
}
