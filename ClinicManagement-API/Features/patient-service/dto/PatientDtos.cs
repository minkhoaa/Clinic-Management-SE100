#nullable enable
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;

namespace ClinicManagement_API.Features.patient_service.dto;

public record PatientDto(
    Guid PatientId, 
    Guid ClinicId, 
    string PatientCode, 
    string FullName, 
    Gender Gender, 
    DateTime? Dob, 
    string? PrimaryPhone, 
    string? Email, 
    string? Note, 
    string? AddressLine1, 
    Clinic Clinic);

public record CreatePatientDto(
    Guid ClinicId, 
    string PatientCode, 
    Gender Gender, 
    string FullName, 
    string? PrimaryPhone, 
    string? Email, 
    string? AddressLine1, 
    DateTime? Dob, 
    string? Note);

public record PatientProfileResponse(
    Guid Id,
    string FullName,
    string Gender,
    string? Dob,
    string? Phone,
    string? Email,
    string? Address,
    string? Allergy,
    string? ChronicDisease,
    string? EmergencyName,
    string? EmergencyPhone,
    string? BloodGroup,
    string? InsuranceType,
    string? InsuranceNumber);

public record UpdatePatientProfileRequest(
    string FullName,
    string Gender,
    string? Dob,
    string? Phone,
    string? Email,
    string? Address,
    string? EmergencyName,
    string? EmergencyPhone,
    string? BloodGroup,
    string? Allergy,
    string? ChronicDisease,
    string? InsuranceType,
    string? InsuranceNumber);

// Appointment DTOs
public record AppointmentListItemDto(
    Guid Id,
    string Title,           // Service name
    string Doctor,          // Doctor name
    string Date,            // DD/MM/YYYY
    string Time,            // HH:mm
    string? Note,
    string Status);         // confirmed | pending | completed | cancelled

// Request to cancel appointment
public record CancelAppointmentRequest(string Reason);
