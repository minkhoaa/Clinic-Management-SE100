using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.doctor_service.dto;
using ClinicManagement_API.Features.audit_service.service;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ClinicManagement_API.Features.doctor_service.service;

public interface IDoctorPracticeService
{
    Task<IResult> GetDashboardStatsAsync(ClaimsPrincipal user);
    Task<IResult> GetQueueAsync(ClaimsPrincipal user, string? date);
    Task<IResult> StartExamAsync(ClaimsPrincipal user, Guid appointmentId);
    Task<IResult> CompleteExamAsync(ClaimsPrincipal user, Guid appointmentId);
    Task<IResult> GetPatientsAsync(ClaimsPrincipal user);
    Task<IResult> GetPatientDetailAsync(ClaimsPrincipal user, Guid patientId);
    Task<IResult> GetMedicalRecordDetailAsync(Guid recordId);
    Task<IResult> CreateMedicalRecordAsync(ClaimsPrincipal user, CreateMedicalRecordRequest request);
    Task<IResult> UpdateMedicalRecordAsync(ClaimsPrincipal user, Guid recordId, UpdateMedicalRecordRequest request);
    Task<IResult> GetPrescriptionTemplatesAsync(ClaimsPrincipal user);
    Task<IResult> CreatePrescriptionTemplateAsync(ClaimsPrincipal user, CreatePrescriptionTemplateRequest request);
    Task<IResult> CreateExaminationAsync(ClaimsPrincipal user, CreateExaminationRequest request);

    // Attachment APIs
    Task<IResult> UploadAttachmentAsync(ClaimsPrincipal user, Guid recordId, IFormFile file, string? description);
    Task<IResult> DeleteAttachmentAsync(ClaimsPrincipal user, Guid recordId, Guid attachmentId);
    Task<IResult> GetAttachmentsAsync(Guid recordId);
}

public class DoctorPracticeService : IDoctorPracticeService
{
    private readonly ClinicDbContext _context;
    private readonly IAuditService _auditService;

    public DoctorPracticeService(ClinicDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    private async Task<Guid?> GetDoctorIdFromUser(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        // Find StaffUser linked to this user
        var staffUser = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Role == AppRoles.Doctor);

        if (staffUser == null)
            return null;

        // Find doctor in the same clinic with matching name
        var doctor = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClinicId == staffUser.ClinicId && d.FullName == staffUser.FullName);

        return doctor?.DoctorId;
    }

    public async Task<IResult> GetDashboardStatsAsync(ClaimsPrincipal user)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value);

        // Waiting count (Confirmed or CheckedIn today)
        var waitingCount = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.CheckedIn)
            .CountAsync();

        // Examined today (Completed today)
        var examinedToday = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Completed)
            .CountAsync();

        // Total appointments today
        var appointmentsToday = await query
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .CountAsync();

        // Average exam time (placeholder - would need actual start/end times)
        var averageExamTime = "30 phút";

        // Waiting queue (top 10)
        var waitingAppointments = await query
            .Include(a => a.Service)
            .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
            .Where(a => a.Status == AppointmentStatus.Confirmed ||
                        a.Status == AppointmentStatus.CheckedIn ||
                        a.Status == AppointmentStatus.InProgress)
            .OrderBy(a => a.StartAt)
            .Take(10)
            .Select(a => new
            {
                a.AppointmentId,
                a.ContactFullName,
                ServiceName = a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.StartAt,
                a.Status
            })
            .ToListAsync();

        var waitingQueue = waitingAppointments.Select((a, index) => new DoctorQueueItemDto(
            a.AppointmentId,
            index + 1,
            a.ContactFullName,
            a.ServiceName,
            a.StartAt.ToString("HH:mm"),
            MapQueueStatus(a.Status)
        )).ToList();

        var stats = new DoctorDashboardStatsDto(
            waitingCount,
            examinedToday,
            averageExamTime,
            appointmentsToday,
            waitingQueue
        );

        return Results.Ok(new ApiResponse<DoctorDashboardStatsDto>(true, "Dashboard stats retrieved", stats));
    }

    public async Task<IResult> GetQueueAsync(ClaimsPrincipal user, string? date)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Parse date or use today
        DateOnly targetDate;
        if (!string.IsNullOrEmpty(date))
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", out targetDate))
            {
                return Results.BadRequest(new ApiResponse<object>(false, "Invalid date format. Use YYYY-MM-DD", null));
            }
        }
        else
        {
            targetDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        var startOfDay = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = targetDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var appointments = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Patient)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value)
            .Where(a => a.StartAt >= startOfDay && a.StartAt <= endOfDay)
            .OrderBy(a => a.StartAt)
            .Select(a => new
            {
                a.AppointmentId,
                a.PatientId,
                a.ContactFullName,
                a.ContactPhone,
                ServiceName = a.Service != null ? a.Service.Name : "Khám tổng quát",
                a.ServiceId,
                a.StartAt,
                a.ActualStartTime,
                a.ActualEndTime,
                a.Status
            })
            .ToListAsync();

        var result = appointments.Select((a, index) => new DoctorQueueDetailDto(
            a.AppointmentId,
            index + 1,
            a.AppointmentId,
            a.PatientId,
            a.ContactFullName,
            a.ContactPhone,
            a.ServiceName,
            a.ServiceId,
            a.StartAt,
            a.ActualStartTime,
            a.ActualEndTime,
            MapQueueStatus(a.Status)
        )).ToList();

        return Results.Ok(new ApiResponse<List<DoctorQueueDetailDto>>(true, "Queue retrieved", result));
    }

    public async Task<IResult> GetPatientsAsync(ClaimsPrincipal user)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Get unique patients from completed appointments for this doctor
        var patients = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId.Value && a.PatientId != null)
            .Where(a => a.Status == AppointmentStatus.Completed)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                LastVisit = g.Max(a => a.StartAt),
                TotalVisits = g.Count()
            })
            .ToListAsync();

        var patientIds = patients.Select(p => p.PatientId).ToList();

        var patientDetails = await _context.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.PatientId))
            .ToDictionaryAsync(p => p.PatientId);

        var result = patients
            .Where(p => p.PatientId != null && patientDetails.ContainsKey(p.PatientId.Value))
            .Select(p =>
            {
                var patient = patientDetails[p.PatientId!.Value];
                return new DoctorPatientListItemDto(
                    patient.PatientId,
                    patient.FullName,
                    patient.PrimaryPhone ?? "",
                    patient.Email,
                    p.LastVisit,
                    p.TotalVisits
                );
            })
            .OrderByDescending(p => p.LastVisit)
            .ToList();

        return Results.Ok(new ApiResponse<List<DoctorPatientListItemDto>>(true, "Patients retrieved", result));
    }

    public async Task<IResult> GetPatientDetailAsync(ClaimsPrincipal user, Guid patientId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Get medical history
        var medicalHistory = await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Doctor)
            .Include(m => m.Appointment)
            .ThenInclude(a => a!.Service)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.RecordDate)
            .Take(20)
            .Select(m => new DoctorMedicalHistoryItemDto(
                m.RecordId,
                m.RecordDate,
                m.Doctor.FullName,
                m.Appointment != null && m.Appointment.Service != null
                    ? m.Appointment.Service.Name
                    : "Khám tổng quát",
                m.Diagnosis,
                m.Notes
            ))
            .ToListAsync();

        // Get upcoming appointments
        var now = DateTime.UtcNow;
        var upcomingAppointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Where(a => a.PatientId == patientId && a.StartAt >= now)
            .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.StartAt)
            .Take(5)
            .Select(a => new DoctorUpcomingAppointmentDto(
                a.AppointmentId,
                a.StartAt,
                a.Service != null ? a.Service.Name : "Khám tổng quát",
                MapQueueStatusStatic(a.Status)
            ))
            .ToListAsync();

        // Calculate stats
        var lastVisit = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.StartAt)
            .Select(a => a.StartAt)
            .FirstOrDefaultAsync();

        var totalVisits = await _context.Appointments
            .AsNoTracking()
            .CountAsync(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed);

        var result = new DoctorPatientDetailDto(
            patient.PatientId,
            patient.FullName,
            patient.PrimaryPhone ?? "",
            patient.Email,
            patient.Dob,
            patient.Gender.ToString(),
            patient.AddressLine1,
            patient.Allergy,
            patient.ChronicDisease,
            lastVisit != default ? lastVisit : null,
            totalVisits,
            medicalHistory,
            upcomingAppointments
        );

        return Results.Ok(new ApiResponse<DoctorPatientDetailDto>(true, "Patient detail retrieved", result));
    }

    public async Task<IResult> GetMedicalRecordDetailAsync(Guid recordId)
    {
        var record = await _context.MedicalRecords
            .Include(r => r.Doctor)
            .Include(r => r.Attachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecordId == recordId);

        if (record == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medical record not found", null));

        var response = new DoctorMedicalRecordDetailDto(
            record.RecordId,
            record.Title,
            record.Doctor.FullName,
            record.RecordDate,
            record.Diagnosis,
            record.Treatment,
            record.Prescription,
            record.Notes,
            new List<object>() // Attachments empty for now
        );

        return Results.Ok(new ApiResponse<DoctorMedicalRecordDetailDto>(true, "Medical record retrieved", response));
    }

    private string MapQueueStatus(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "pending",
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.CheckedIn => "checkedin",
        AppointmentStatus.InProgress => "inprogress",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.NoShow => "noshow",
        _ => "unknown"
    };

    private static string MapQueueStatusStatic(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "pending",
        AppointmentStatus.Confirmed => "confirmed",
        AppointmentStatus.CheckedIn => "checkedin",
        AppointmentStatus.InProgress => "inprogress",
        AppointmentStatus.Completed => "completed",
        AppointmentStatus.Cancelled => "cancelled",
        AppointmentStatus.NoShow => "noshow",
        _ => "unknown"
    };

    public async Task<IResult> StartExamAsync(ClaimsPrincipal user, Guid appointmentId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));

        if (appointment.DoctorId != doctorId.Value)
            return Results.Forbid();

        if (appointment.Status != AppointmentStatus.CheckedIn && appointment.Status != AppointmentStatus.Confirmed)
            return Results.BadRequest(new ApiResponse<object>(false,
                "Appointment must be checked-in or confirmed to start exam", null));

        appointment.Status = AppointmentStatus.InProgress;
        appointment.ActualStartTime = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Exam started", new
        {
            appointment.AppointmentId,
            Status = "inprogress",
            StartTime = appointment.ActualStartTime
        }));
    }

    public async Task<IResult> CompleteExamAsync(ClaimsPrincipal user, Guid appointmentId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));

        if (appointment.DoctorId != doctorId.Value)
            return Results.Forbid();

        if (appointment.Status != AppointmentStatus.InProgress)
            return Results.BadRequest(new ApiResponse<object>(false, "Appointment must be in-progress to complete",
                null));

        appointment.Status = AppointmentStatus.Completed;
        appointment.ActualEndTime = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        // Calculate duration
        var duration = appointment.ActualStartTime.HasValue
            ? (appointment.ActualEndTime.Value - appointment.ActualStartTime.Value).TotalMinutes
            : 0;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Exam completed", new
        {
            appointment.AppointmentId,
            Status = "completed",
            EndTime = appointment.ActualEndTime,
            Duration = $"{(int)duration} minutes"
        }));
    }

    public async Task<IResult> CreateMedicalRecordAsync(ClaimsPrincipal user, CreateMedicalRecordRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Verify patient exists
        var patient = await _context.Patients.FindAsync(request.PatientId);
        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Get doctor's clinic
        var doctor = await _context.Doctors.FindAsync(doctorId.Value);
        if (doctor == null)
            return Results.NotFound(new ApiResponse<object>(false, "Doctor not found", null));

        var record = new MedicalRecord
        {
            RecordId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ClinicId = doctor.ClinicId,
            DoctorId = doctorId.Value,
            AppointmentId = request.AppointmentId,
            Title = request.Title,
            RecordDate = DateTime.UtcNow,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Prescription = request.Prescription,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        // Audit log for created medical record
        await _auditService.LogAsync(
            doctor.ClinicId,
            AuditEntityType.MedicalRecord,
            record.RecordId,
            AuditAction.Created,
            user,
            null,
            new { record.PatientId, record.Title, record.Diagnosis, record.Treatment },
            $"Tạo hồ sơ bệnh án: {record.Title}"
        );

        return Results.Created($"/api/doctor/medical-records/{record.RecordId}",
            new ApiResponse<CreateMedicalRecordResponse>(true, "Medical record created",
                new CreateMedicalRecordResponse(record.RecordId, record.CreatedAt)));
    }

    public async Task<IResult> UpdateMedicalRecordAsync(ClaimsPrincipal user, Guid recordId,
        UpdateMedicalRecordRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var record = await _context.MedicalRecords.FindAsync(recordId);
        if (record == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medical record not found", null));

        // Only allow doctor who created to edit
        if (record.DoctorId != doctorId.Value)
            return Results.Forbid();

        // Save old values for audit
        var oldValues = new
        {
            record.PatientId,
            record.AppointmentId,
            record.Title,
            record.Diagnosis,
            record.Treatment,
            record.Prescription,
            record.Notes
        };

        record.AppointmentId = request.AppointmentId;
        record.PatientId = request.PatientId;
        record.Title = request.Title;
        record.Diagnosis = request.Diagnosis;
        record.Treatment = request.Treatment;
        record.Prescription = request.Prescription;
        record.Notes = request.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log for updated medical record
        await _auditService.LogAsync(
            record.ClinicId,
            AuditEntityType.MedicalRecord,
            record.RecordId,
            AuditAction.Updated,
            user,
            oldValues,
            new
            {
                record.PatientId, record.Title, record.Diagnosis, record.Treatment, record.Prescription, record.Notes
            },
            $"Cập nhật hồ sơ bệnh án: {record.Title}"
        );

        return Results.Ok(new ApiResponse<UpdateMedicalRecordResponse>(true, "Medical record updated",
            new UpdateMedicalRecordResponse(record.RecordId, record.UpdatedAt)));
    }

    public async Task<IResult> GetPrescriptionTemplatesAsync(ClaimsPrincipal user)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var doctor = await _context.Doctors.FindAsync(doctorId.Value);
        if (doctor == null)
            return Results.NotFound(new ApiResponse<object>(false, "Doctor not found", null));

        // Get templates with medicines loaded from junction table
        var templates = await _context.PrescriptionTemplates
            .AsNoTracking()
            .Include(t => t.PrescriptionTemplateMedicines)
            .ThenInclude(ptm => ptm.Medicine)
            .Where(t => t.ClinicId == doctor.ClinicId)
            .Where(t => t.DoctorId == null || t.DoctorId == doctorId.Value)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var result = templates.Select(t => new PrescriptionTemplateDto(
            t.TemplateId,
            t.Name,
            t.Category,
            t.PrescriptionTemplateMedicines.Select(ptm => new MedicineDto(
                ptm.MedicineId,
                ptm.Medicine.Name,
                ptm.Dosage,
                ptm.Quantity.ToString(),
                ptm.Instructions
            )).ToList(),
            t.Notes
        )).ToList();

        return Results.Ok(new ApiResponse<List<PrescriptionTemplateDto>>(true, "Templates retrieved", result));
    }

    public async Task<IResult> CreatePrescriptionTemplateAsync(ClaimsPrincipal user,
        CreatePrescriptionTemplateRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var doctor = await _context.Doctors.FindAsync(doctorId.Value);
        if (doctor == null)
            return Results.NotFound(new ApiResponse<object>(false, "Doctor not found", null));

        // Validate all MedicineIds exist and belong to the same clinic
        var medicineIds = request.Medicines.Select(m => m.MedicineId).ToList();

        var existingMedicines = await _context.Medicines
            .Where(m => medicineIds.Contains(m.MedicineId) && m.ClinicId == doctor.ClinicId && m.IsActive)
            .Select(m => m.MedicineId)
            .ToListAsync();

        var missingIds = medicineIds.Except(existingMedicines).ToList();
        if (missingIds.Any())
        {
            return Results.BadRequest(new ApiResponse<object>(false,
                $"Some medicines not found or inactive: {string.Join(", ", missingIds)}", null));
        }

        // Create template
        var template = new PrescriptionTemplate
        {
            TemplateId = Guid.NewGuid(),
            ClinicId = doctor.ClinicId,
            DoctorId = doctorId.Value,
            Name = request.Name,
            Category = request.Category,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PrescriptionTemplates.Add(template);

        // Create junction table records
        var templateMedicines = request.Medicines.Select(m => new PrescriptionTemplateMedicine
        {
            Id = Guid.NewGuid(),
            TemplateId = template.TemplateId,
            MedicineId = m.MedicineId,
            Dosage = m.Dosage,
            Quantity = int.Parse(m.Quantity),
            Instructions = m.Instructions
        }).ToList();

        _context.PrescriptionTemplateMedicines.AddRange(templateMedicines);
        await _context.SaveChangesAsync();

        return Results.Created($"/api/doctor/prescription-templates/{template.TemplateId}",
            new ApiResponse<CreatePrescriptionTemplateResponse>(true, "Template created",
                new CreatePrescriptionTemplateResponse(template.TemplateId, template.CreatedAt)));
    }

    public async Task<IResult> CreateExaminationAsync(ClaimsPrincipal user, CreateExaminationRequest request)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Validate patient exists
        var patient = await _context.Patients.FindAsync(request.PatientId);
        if (patient == null)
            return Results.NotFound(new ApiResponse<object>(false, "Patient not found", null));

        // Get doctor info
        var doctor = await _context.Doctors.FindAsync(doctorId.Value);
        if (doctor == null)
            return Results.NotFound(new ApiResponse<object>(false, "Doctor not found", null));

        // Prepare prescription JSON
        string? prescriptionJson = null;
        if (request.Prescription != null)
        {
            prescriptionJson = JsonSerializer.Serialize(request.Prescription);
        }

        // Prepare notes with ToothStatus if present
        string? notes = request.Notes;
        if (request.ToothStatus != null && request.ToothStatus.Count > 0)
        {
            var toothStatusJson = JsonSerializer.Serialize(request.ToothStatus);
            notes = $"{request.Notes ?? ""}\n[ToothStatus]: {toothStatusJson}".Trim();
        }

        // Create MedicalRecord
        var record = new MedicalRecord
        {
            RecordId = Guid.NewGuid(),
            PatientId = request.PatientId,
            ClinicId = doctor.ClinicId,
            DoctorId = doctorId.Value,
            AppointmentId = request.AppointmentId,
            Title = request.Title,
            RecordDate = DateTime.UtcNow,
            Diagnosis = request.Diagnosis,
            Treatment = request.Treatment,
            Prescription = prescriptionJson,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);

        Guid? billId = null;

        // Create Bill if requested
        if (request is { CreateBill: true, ServiceIds.Count: > 0 })
        {
            // Get services and calculate total
            var services = await _context.Services
                .AsNoTracking()
                .Where(s => request.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            var subtotal = services.Sum(s => s.DefaultPrice ?? 0);

            // Generate invoice number
            var billCount = await _context.Bills.AsNoTracking().CountAsync(b => b.ClinicId == doctor.ClinicId);
            var invoiceNumber = $"HD-{DateTime.UtcNow:yyyy}-{(billCount + 1):D4}";

            var bill = new Bill
            {
                BillId = Guid.NewGuid(),
                ClinicId = doctor.ClinicId,
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                MedicalRecordId = record.RecordId,
                InvoiceNumber = invoiceNumber,
                Status = BillStatus.Pending,
                Subtotal = subtotal,
                Discount = 0,
                TotalAmount = subtotal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bills.Add(bill);

            // Create BillItems
            var billItems = services.Select(k => new BillItem()
            {
                BillItemId = Guid.NewGuid(),
                BillId = bill.BillId,
                ServiceId = k.ServiceId,
                Name = k.Name,
                Quantity = 1,
                UnitPrice = k.DefaultPrice ?? 0,
                Amount = k.DefaultPrice ?? 0
            });

            _context.BillItems.AddRange(billItems);

            billId = bill.BillId;
        }

        if (request.AppointmentId != Guid.Empty)
        {
            var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
            if (appointment != null && appointment.Status != AppointmentStatus.Completed)
            {
                appointment.Status = AppointmentStatus.Completed;
                appointment.ActualEndTime = DateTime.UtcNow;
                appointment.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return Results.Created($"/api/doctor/medical-records/{record.RecordId}",
            new ApiResponse<CreateExaminationResponse>(true, "Examination created",
                new CreateExaminationResponse(record.RecordId, record.RecordId, billId, record.CreatedAt)));
    }

    // ===== Attachment APIs =====

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".dcm", ".dicom" };
    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

    public async Task<IResult> UploadAttachmentAsync(ClaimsPrincipal user, Guid recordId, IFormFile file,
        string? description)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        // Validate file
        if (file == null || file.Length == 0)
            return Results.BadRequest(new ApiResponse<object>(false, "No file uploaded", null));

        if (file.Length > MaxFileSize)
            return Results.BadRequest(new ApiResponse<object>(false,
                $"File size exceeds limit of {MaxFileSize / 1024 / 1024}MB", null));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return Results.BadRequest(new ApiResponse<object>(false,
                $"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}", null));

        // Verify medical record exists and belongs to doctor's clinic
        var record = await _context.MedicalRecords
            .Include(r => r.Doctor)
            .FirstOrDefaultAsync(r => r.RecordId == recordId);

        if (record == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medical record not found", null));

        // Create upload directory
        var uploadDir = Path.Combine("uploads", "medical-records", recordId.ToString());
        Directory.CreateDirectory(uploadDir);

        // Generate unique filename
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadDir, storedFileName);

        // Save file
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create attachment record
        var attachment = new MedicalRecordAttachment
        {
            AttachmentId = Guid.NewGuid(),
            RecordId = recordId,
            FileName = file.FileName,
            StoredFileName = Path.Combine(recordId.ToString(), storedFileName),
            ContentType = file.ContentType,
            FileSize = file.Length,
            Description = description,
            UploadedAt = DateTime.UtcNow
        };

        _context.MedicalRecordAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<AttachmentUploadResponse>(true, "File uploaded successfully",
            new AttachmentUploadResponse(
                attachment.AttachmentId,
                attachment.FileName,
                attachment.ContentType,
                attachment.FileSize,
                attachment.Description,
                attachment.UploadedAt
            )));
    }

    public async Task<IResult> DeleteAttachmentAsync(ClaimsPrincipal user, Guid recordId, Guid attachmentId)
    {
        var doctorId = await GetDoctorIdFromUser(user);
        if (doctorId == null)
            return Results.Unauthorized();

        var attachment = await _context.MedicalRecordAttachments
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.RecordId == recordId);

        if (attachment == null)
            return Results.NotFound(new ApiResponse<object>(false, "Attachment not found", null));

        // Delete physical file
        var filePath = Path.Combine("uploads", "medical-records", attachment.StoredFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Delete database record
        _context.MedicalRecordAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Attachment deleted successfully", null));
    }

    public async Task<IResult> GetAttachmentsAsync(Guid recordId)
    {
        var attachments = await _context.MedicalRecordAttachments
            .AsNoTracking()
            .Where(a => a.RecordId == recordId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentUploadResponse(
                a.AttachmentId,
                a.FileName,
                a.ContentType,
                a.FileSize,
                a.Description,
                a.UploadedAt
            ))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<AttachmentUploadResponse>>(true, "Attachments retrieved", attachments));
    }
}
