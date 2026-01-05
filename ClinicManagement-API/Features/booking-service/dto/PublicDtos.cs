#nullable enable
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Features.booking_service.dto;

public record ClinicDto(Guid ClinicId, string Code, string Name, string TimeZone, string? Phone, string? Email);
public record ServiceDto(Guid ServiceId, string Code, string Name, short? DefaultDurationMin, decimal? DefaultPrice, bool IsActive, Guid ClinicId);
public record DoctorDto(Guid DoctorId, Guid ClinicId, string Code, string FullName, string? Specialty, string? Phone, string? Email, bool IsActive);
public record AvailabilityDto(DateOnly Date, TimeSpan StartTime, TimeSpan EndTime, short SlotSizeMin);
public record SlotDto(DateTime StartAt, DateTime EndAt);
public record EnumDto(int Value, string Name);

public record CreateBookingRequest(
    Guid ClinicId,
    Guid DoctorId,
    Guid? ServiceId,
    Guid? PatientId,
    DateTime StartAt,
    DateTime EndAt,
    string FullName,
    string Phone,
    string? Email,
    string? Notes,
    AppointmentSource? Channel);

public record BookingResponse(Guid BookingId, BookingStatus Status, string? CancelToken, string? RescheduleToken);
public record AppointmentResponse(Guid AppointmentId, AppointmentStatus Status);

public record CreateServiceRequest(Guid ClinicId, string Code, string Name, short? DefaultDurationMin, decimal? DefaultPrice, bool IsActive);

public record StaffUserDto(Guid UserId, Guid ClinicId, string Username, string FullName, StaffRole Role, bool IsActive, Clinic Clinic);

public record CreateStaffUserDto(Guid ClinicId, string Username, string FullName, StaffRole Role, bool IsActive);

public record PatientDto(Guid PatientId, Guid ClinicId, string PatientCode, string FullName, Gender Gender, DateTime? Dob, string? PrimaryPhone, string? Email, string? AddressLine1, string? Note, Clinic Clinic);

public record CreatePatientDto(Guid ClinicId, string PatientCode, Gender Gender, string FullName, string? PrimaryPhone, string? Email, string? AddressLine1, DateTime? Dob, string? Note);

public record DoctorTimeOffDto(Guid TimeOffId, Guid ClinicId, Guid DoctorId, DateTime StartAt, DateTime EndAt, string? Reason, Clinic Clinic, Doctor Doctor);
public record AddDoctorTimeOffRequest(Guid ClinicId, Guid DoctorId, DateTime StartAt, DateTime EndAt, string? Reason);

