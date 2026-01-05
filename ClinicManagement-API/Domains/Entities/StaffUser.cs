using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Domains.Entities
{
    public class StaffUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }
        public Guid ClinicId { get; set; }
        public string Username { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public StaffRole Role { get; set; } = StaffRole.Receptionist;
        public bool IsActive { get; set; } = true;
        public Clinic Clinic { get; set; } = default!;
    }
}