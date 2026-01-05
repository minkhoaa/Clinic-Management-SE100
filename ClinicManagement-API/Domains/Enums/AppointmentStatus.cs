namespace ClinicManagement_API.Domains.Enums
{
    public enum AppointmentStatus : short
    {
        Booked = 1,
        Confirmed = 2,
        CheckedIn = 3,
        InProgress = 4,
        Completed = 5,
        Cancelled = 6,
        NoShow = 7,
        Rescheduling = 8
    }
    public enum AppointmentSource : byte { Web = 1, App = 2, Hotline = 3, FrontDesk = 4 }
    public enum StaffRole : byte { Receptionist = 1, Doctor = 2, Admin = 3 }

}
