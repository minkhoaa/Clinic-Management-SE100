#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Infrastructure.Persisstence;

namespace ClinicManagement_API.Domains.Entities;

/// <summary>
/// Audit trail for tracking changes to sensitive entities (MedicalRecords, etc.).
/// Complies with healthcare data regulations for maintaining change history.
/// </summary>
public sealed class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AuditLogId { get; set; }
    
    public Guid ClinicId { get; set; }
    
    /// <summary>
    /// Type of entity being audited.
    /// </summary>
    public AuditEntityType EntityType { get; set; }
    
    /// <summary>
    /// ID of the entity being audited.
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Action performed on the entity.
    /// </summary>
    public AuditAction Action { get; set; }
    
    /// <summary>
    /// User who performed the action.
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Name of the user for display purposes.
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Role of the user when action was performed.
    /// </summary>
    public string? UserRole { get; set; }
    
    /// <summary>
    /// Previous values before change (JSON format).
    /// </summary>
    public string? OldValues { get; set; }
    
    /// <summary>
    /// New values after change (JSON format).
    /// </summary>
    public string? NewValues { get; set; }
    
    /// <summary>
    /// Summary of what changed (human-readable).
    /// </summary>
    public string? ChangesSummary { get; set; }
    
    /// <summary>
    /// IP address of the request (for security).
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent of the request.
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Timestamp of the action.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Clinic Clinic { get; set; } = default!;
    public User? User { get; set; }
}
