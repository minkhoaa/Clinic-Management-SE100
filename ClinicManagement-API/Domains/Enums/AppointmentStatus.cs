namespace ClinicManagement_API.Domains.Enums
{
    public enum AppointmentStatus : short
    {
        Pending = 0,        // Chờ xác nhận
        Confirmed = 1,      // Đã xác nhận
        CheckedIn = 2,      // Đã check-in
        InProgress = 3,     // Đang khám
        Completed = 4,      // Hoàn thành
        Cancelled = 5,      // Đã hủy
        NoShow = 6,         // Không đến
        Rescheduling = 7    // Đang dời lịch
    }
    
    public enum AppointmentSource : byte { Web = 1, App = 2, Hotline = 3, FrontDesk = 4 }
}

