using System.Security.Claims;
using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.tooth_record_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.tooth_record_service.service;

public interface IToothRecordService
{
    Task<IResult> GetDentalChartAsync(ClaimsPrincipal user, Guid patientId);
    Task<IResult> UpdateToothRecordAsync(ClaimsPrincipal user, Guid patientId, UpdateToothRecordRequest request);
    Task<IResult> BatchUpdateToothRecordsAsync(ClaimsPrincipal user, BatchUpdateToothRecordsRequest request);
    Task<IResult> GetToothHistoryAsync(ClaimsPrincipal user, Guid patientId, int toothNumber);
}

public class ToothRecordService : IToothRecordService
{
    private readonly ClinicDbContext _context;

    public ToothRecordService(ClinicDbContext context)
    {
        _context = context;
    }

    private async Task<Guid?> GetClinicIdFromUser(ClaimsPrincipal user)
    {
        var clinicIdClaim = user.FindFirst("ClinicId")?.Value;
        if (Guid.TryParse(clinicIdClaim, out var clinicId))
            return clinicId;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return null;

        var staff = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userGuid);

        return staff?.ClinicId;
    }

    private async Task<Guid?> GetDoctorIdFromUser(ClaimsPrincipal user)
    {
        var doctorIdClaim = user.FindFirst("DoctorId")?.Value;
        if (Guid.TryParse(doctorIdClaim, out var doctorId))
            return doctorId;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return null;

        // Find StaffUser linked to this user
        var staffUser = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userGuid && s.Role == "DOCTOR");

        if (staffUser == null)
            return null;

        // Find doctor in the same clinic with matching name
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClinicId == staffUser.ClinicId && d.FullName == staffUser.FullName);

        return doctor?.DoctorId;
    }

    public async Task<IResult> GetDentalChartAsync(ClaimsPrincipal user, Guid patientId)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId && p.ClinicId == clinicId);

        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        var toothRecords = await _context.ToothRecords
            .AsNoTracking()
            .Include(t => t.LastTreatedByDoctor)
            .Where(t => t.PatientId == patientId)
            .OrderBy(t => t.ToothNumber)
            .ToListAsync();

        var teeth = toothRecords.Select(t => new ToothRecordDto(
            t.ToothRecordId,
            t.ToothNumber,
            GetToothName(t.ToothNumber),
            GetQuadrant(t.ToothNumber),
            t.Status,
            GetStatusName(t.Status),
            t.PreviousStatus,
            t.Notes,
            t.LastTreatment,
            t.LastTreatedAt,
            t.LastTreatedByDoctor?.FullName
        )).ToList();

        var chart = new DentalChartDto(
            patientId,
            patient.FullName,
            teeth,
            toothRecords.Max(t => (DateTime?)t.UpdatedAt)
        );

        return Results.Ok(new ApiResponse<DentalChartDto>(true, "Dental chart retrieved", chart));
    }

    public async Task<IResult> UpdateToothRecordAsync(ClaimsPrincipal user, Guid patientId,
        UpdateToothRecordRequest request)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var doctorId = await GetDoctorIdFromUser(user);

        // Validate tooth number (FDI notation: 11-18, 21-28, 31-38, 41-48)
        if (!IsValidToothNumber(request.ToothNumber))
            return Results.BadRequest(new ApiResponse<object>(false,
                "Invalid tooth number. Use FDI notation (11-18, 21-28, 31-38, 41-48)", null));

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == patientId && p.ClinicId == clinicId);

        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        var toothRecord = await _context.ToothRecords
            .FirstOrDefaultAsync(t => t.PatientId == patientId && t.ToothNumber == request.ToothNumber);

        if (toothRecord == null)
        {
            // Create new record
            toothRecord = new ToothRecord
            {
                ToothRecordId = Guid.NewGuid(),
                PatientId = patientId,
                ClinicId = clinicId.Value,
                ToothNumber = request.ToothNumber,
                Status = request.Status,
                Notes = request.Notes,
                LastTreatment = request.LastTreatment,
                LastMedicalRecordId = request.MedicalRecordId,
                LastTreatedAt = DateTime.UtcNow,
                LastTreatedByDoctorId = doctorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ToothRecords.Add(toothRecord);
        }
        else
        {
            // Update existing
            toothRecord.PreviousStatus = toothRecord.Status;
            toothRecord.Status = request.Status;
            toothRecord.Notes = request.Notes;
            toothRecord.LastTreatment = request.LastTreatment;
            toothRecord.LastMedicalRecordId = request.MedicalRecordId;
            toothRecord.LastTreatedAt = DateTime.UtcNow;
            toothRecord.LastTreatedByDoctorId = doctorId;
            toothRecord.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<ToothRecordResponse>(true, "Tooth record updated",
            new ToothRecordResponse(toothRecord.ToothRecordId, toothRecord.ToothNumber, toothRecord.Status,
                toothRecord.UpdatedAt)));
    }

    public async Task<IResult> BatchUpdateToothRecordsAsync(ClaimsPrincipal user,
        BatchUpdateToothRecordsRequest request)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var doctorId = await GetDoctorIdFromUser(user);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == request.PatientId && p.ClinicId == clinicId);

        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Validate all tooth numbers
        var invalidTeeth = request.Teeth.Where(t => !IsValidToothNumber(t.ToothNumber)).ToList();
        if (invalidTeeth.Any())
            return Results.BadRequest(new ApiResponse<object>(false,
                $"Invalid tooth numbers: {string.Join(", ", invalidTeeth.Select(t => t.ToothNumber))}", null));

        var existingRecords = await _context.ToothRecords
            .Where(t => t.PatientId == request.PatientId)
            .ToDictionaryAsync(t => t.ToothNumber);

        var updatedCount = 0;
        var createdCount = 0;

        foreach (var tooth in request.Teeth)
        {
            if (existingRecords.TryGetValue(tooth.ToothNumber, out var existing))
            {
                existing.PreviousStatus = existing.Status;
                existing.Status = tooth.Status;
                existing.Notes = tooth.Notes;
                existing.LastTreatment = tooth.Treatment;
                existing.LastMedicalRecordId = request.MedicalRecordId;
                existing.LastTreatedAt = DateTime.UtcNow;
                existing.LastTreatedByDoctorId = doctorId;
                existing.UpdatedAt = DateTime.UtcNow;
                updatedCount++;
            }
            else
            {
                var newRecord = new ToothRecord
                {
                    ToothRecordId = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    ClinicId = clinicId.Value,
                    ToothNumber = tooth.ToothNumber,
                    Status = tooth.Status,
                    Notes = tooth.Notes,
                    LastTreatment = tooth.Treatment,
                    LastMedicalRecordId = request.MedicalRecordId,
                    LastTreatedAt = DateTime.UtcNow,
                    LastTreatedByDoctorId = doctorId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ToothRecords.Add(newRecord);
                createdCount++;
            }
        }

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true,
            $"Batch update complete. Created: {createdCount}, Updated: {updatedCount}",
            new { CreatedCount = createdCount, UpdatedCount = updatedCount }));
    }

    public async Task<IResult> GetToothHistoryAsync(ClaimsPrincipal user, Guid patientId, int toothNumber)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var tooth = await _context.ToothRecords
            .AsNoTracking()
            .Include(t => t.LastTreatedByDoctor)
            .Include(t => t.LastMedicalRecord)
            .FirstOrDefaultAsync(t => t.PatientId == patientId && t.ToothNumber == toothNumber);

        if (tooth == null)
            return Results.NotFound(new ApiResponse<object>(false, "Tooth record not found", null));

        var dto = new ToothRecordDto(
            tooth.ToothRecordId,
            tooth.ToothNumber,
            GetToothName(tooth.ToothNumber),
            GetQuadrant(tooth.ToothNumber),
            tooth.Status,
            GetStatusName(tooth.Status),
            tooth.PreviousStatus,
            tooth.Notes,
            tooth.LastTreatment,
            tooth.LastTreatedAt,
            tooth.LastTreatedByDoctor?.FullName
        );

        return Results.Ok(new ApiResponse<ToothRecordDto>(true, "Tooth record retrieved", dto));
    }

    // Helper methods
    private static bool IsValidToothNumber(int number)
    {
        // FDI notation: quadrant (1-4) + tooth position (1-8)
        var quadrant = number / 10;
        var position = number % 10;
        return quadrant >= 1 && quadrant <= 4 && position >= 1 && position <= 8;
    }

    private static string GetQuadrant(int number)
    {
        return (number / 10) switch
        {
            1 => "Trên phải",
            2 => "Trên trái",
            3 => "Dưới trái",
            4 => "Dưới phải",
            _ => "Không xác định"
        };
    }

    private static string GetToothName(int number)
    {
        var quadrant = GetQuadrant(number);
        var position = number % 10;
        var toothType = position switch
        {
            1 => "Răng cửa giữa",
            2 => "Răng cửa bên",
            3 => "Răng nanh",
            4 => "Răng cối nhỏ 1",
            5 => "Răng cối nhỏ 2",
            6 => "Răng hàm lớn 1",
            7 => "Răng hàm lớn 2",
            8 => "Răng khôn",
            _ => "Răng"
        };
        return $"{toothType} ({quadrant})";
    }

    private static string GetStatusName(ToothStatus status)
    {
        return status switch
        {
            ToothStatus.Healthy => "Khỏe mạnh",
            ToothStatus.Cavity => "Sâu răng",
            ToothStatus.Filled => "Đã trám",
            ToothStatus.Crown => "Bọc sứ",
            ToothStatus.Missing => "Mất răng",
            ToothStatus.Extracted => "Đã nhổ",
            ToothStatus.RootCanal => "Đã điều trị tủy",
            ToothStatus.Implant => "Cấy ghép implant",
            ToothStatus.Bridge => "Cầu răng",
            ToothStatus.Chipped => "Mẻ răng",
            ToothStatus.Sensitive => "Răng nhạy cảm",
            ToothStatus.Gum => "Vấn đề nướu",
            ToothStatus.Orthodontic => "Đang niềng",
            ToothStatus.PendingTreatment => "Cần điều trị",
            _ => "Không xác định"
        };
    }
}
