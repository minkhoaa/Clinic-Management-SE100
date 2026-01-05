using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagement_API.Domains.Entities
{

    public class DoctorTimeOff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TimeOffId { get; set; }
        public Guid ClinicId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? Reason { get; set; }
        public Clinic Clinic = default!;
        public Doctor Doctor = default!;

    }
}