#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities;

public sealed class MedicalRecordAttachment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AttachmentId { get; set; }
    
    public Guid RecordId { get; set; }
    
    public string FileName { get; set; } = default!;     // Original filename
    public string StoredFileName { get; set; } = default!; // Unique stored filename
    public string ContentType { get; set; } = default!;  // MIME type
    public long FileSize { get; set; }                   // Size in bytes
    public string? Description { get; set; }
    
    public DateTime UploadedAt { get; set; }
    
    // Navigation property
    public MedicalRecord MedicalRecord { get; set; } = default!;
}
