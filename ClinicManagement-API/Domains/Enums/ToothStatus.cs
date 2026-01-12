namespace ClinicManagement_API.Domains.Enums;

/// <summary>
/// Tooth status for dental chart.
/// Using FDI (ISO 3950) notation for tooth numbering (11-18, 21-28, 31-38, 41-48).
/// </summary>
public enum ToothStatus : byte
{
    Healthy = 0,        // Răng khỏe mạnh
    Cavity = 1,         // Sâu răng
    Filled = 2,         // Đã trám
    Crown = 3,          // Bọc sứ/răng giả
    Missing = 4,        // Mất răng
    Extracted = 5,      // Đã nhổ
    RootCanal = 6,      // Đã điều trị tủy
    Implant = 7,        // Cấy ghép implant
    Bridge = 8,         // Cầu răng
    Chipped = 9,        // Mẻ răng
    Sensitive = 10,     // Răng nhạy cảm
    Gum = 11,           // Vấn đề nướu
    Orthodontic = 12,   // Đang niềng
    PendingTreatment = 13  // Cần điều trị
}
