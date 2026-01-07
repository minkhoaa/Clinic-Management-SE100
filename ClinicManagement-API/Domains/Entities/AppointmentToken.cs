#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class AppointmentToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TokenId { get; set; }
    public Guid AppointmentId { get; set; }
    public string Action { get; set; } = default!;  // "Cancel" | "Reschedule"
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }

    public Appointment Appointment { get; set; } = default!;
}
