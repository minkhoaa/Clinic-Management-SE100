using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Features.email_service;
using ClinicManagement_API.Infrastructure.Persisstence;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    public interface IDoctorService
    {
        Task<IResult> CreateDoctorAsync(CreateDoctorRequest request);

        Task<IResult> GetAllDoctorAsync();

        Task<IResult> UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequest request);

        Task<IResult> DeleteDoctorAsync(Guid doctorId);
        Task<IResult> AddDoctorTimeOffAsync(AddDoctorTimeOffRequest request);

        Task<IResult> UpdateDoctorTimeOffAsync(Guid timeOffId, AddDoctorTimeOffRequest request);
        Task<IResult> DeleteDoctorTimeOffAsync(Guid timeOffId);
    }

    public class DoctorService : IDoctorService
    {
        private readonly ClinicDbContext _context;
        private readonly IFluentEmail _email;
        private readonly string _frontendBaseUrl;

        public DoctorService(ClinicDbContext context, IFluentEmail email, IConfiguration config)
        {
            _context = context;
            _email = email;
            _frontendBaseUrl = Environment.GetEnvironmentVariable("EMAIL__FRONTENDBASEURL")
                               ?? config["Email:FrontendBaseUrl"] ?? "http://localhost:3000";
        }

        public async Task<IResult> AddDoctorTimeOffAsync(AddDoctorTimeOffRequest request)
        {
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Doctor not found.", null));
            }

            var clinic = await _context.Clinics.FindAsync(request.ClinicId);
            if (clinic == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Clinic not found.", null));
            }

            // Find affected appointments (Pending, Confirmed, CheckedIn)
            var affectedAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Where(a => a.DoctorId == request.DoctorId
                            && a.StartAt < request.EndAt
                            && a.EndAt > request.StartAt
                            && (a.Status == AppointmentStatus.Pending
                                || a.Status == AppointmentStatus.Confirmed
                                || a.Status == AppointmentStatus.CheckedIn))
                .ToListAsync();

            var timeOff = new DoctorTimeOff
            {
                ClinicId = request.ClinicId,
                DoctorId = request.DoctorId,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Reason = request.Reason
            };
            _context.DoctorTimeOffs.Add(timeOff);
            await _context.SaveChangesAsync();

            // Send notifications to affected patients
            var notifiedPatients = new List<AffectedAppointmentDto>();
            foreach (var appointment in affectedAppointments)
            {
                if (!string.IsNullOrEmpty(appointment.Patient?.Email))
                {
                    try
                    {
                        await SendTimeOffNotificationEmailAsync(
                            appointment.Patient.Email,
                            appointment.Patient.FullName,
                            clinic.Name,
                            doctor.FullName,
                            appointment.Service?.Name ?? "Khám bệnh",
                            appointment.StartAt,
                            appointment.AppointmentId,
                            request.Reason ?? "Lý do cá nhân"
                        );

                        notifiedPatients.Add(new AffectedAppointmentDto(
                            appointment.AppointmentId,
                            appointment.Patient.FullName,
                            appointment.Patient.Email,
                            appointment.Patient.PrimaryPhone,
                            appointment.StartAt,
                            appointment.EndAt,
                            appointment.Status.ToString(),
                            true
                        ));
                    }
                    catch
                    {
                        notifiedPatients.Add(new AffectedAppointmentDto(
                            appointment.AppointmentId,
                            appointment.Patient.FullName,
                            appointment.Patient.Email,
                            appointment.Patient.PrimaryPhone,
                            appointment.StartAt,
                            appointment.EndAt,
                            appointment.Status.ToString(),
                            false
                        ));
                    }
                }
            }

            var result = new TimeOffCreatedResponse(
                timeOff.TimeOffId,
                timeOff.ClinicId,
                timeOff.DoctorId,
                timeOff.StartAt,
                timeOff.EndAt,
                timeOff.Reason,
                clinic.Name,
                doctor.FullName,
                notifiedPatients.Count,
                notifiedPatients
            );

            return Results.Ok(new ApiResponse<TimeOffCreatedResponse>(true,
                $"Đã tạo lịch nghỉ. Có {affectedAppointments.Count} lịch hẹn bị ảnh hưởng.", result));
        }

        private async Task SendTimeOffNotificationEmailAsync(
            string email,
            string patientName,
            string clinicName,
            string doctorName,
            string serviceName,
            DateTime appointmentTime,
            Guid appointmentId,
            string reason)
        {
            var rescheduleUrl = $"{_frontendBaseUrl}/appointment/{appointmentId}/reschedule";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Georgia', serif; line-height: 1.8; color: #2c3e50; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #fff; border: 1px solid #d4d4d4; }}
        .header {{ background-color: #c0392b; color: #fff; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .alert-box {{ background-color: #fdf2f2; border: 1px solid #f5c6cb; padding: 20px; margin: 20px 0; border-radius: 4px; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        .info-table td {{ padding: 10px 0; border-bottom: 1px solid #eee; }}
        .info-table .label {{ color: #6c757d; width: 120px; }}
        .btn {{ display: inline-block; padding: 12px 30px; background-color: #1a3a5c; color: #fff; text-decoration: none; margin-top: 20px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ THÔNG BÁO THAY ĐỔI LỊCH HẸN</h1>
        </div>
        <div class='content'>
            <p>Kính gửi Quý khách <strong>{patientName}</strong>,</p>
            
            <div class='alert-box'>
                <strong>Rất tiếc, lịch hẹn của Quý khách đã bị ảnh hưởng do bác sĩ có lịch nghỉ.</strong>
            </div>
            
            <table class='info-table'>
                <tr><td class='label'>Phòng khám</td><td>{clinicName}</td></tr>
                <tr><td class='label'>Bác sĩ</td><td>{doctorName}</td></tr>
                <tr><td class='label'>Dịch vụ</td><td>{serviceName}</td></tr>
                <tr><td class='label'>Ngày khám</td><td>{appointmentTime:dddd, dd/MM/yyyy}</td></tr>
                <tr><td class='label'>Giờ khám</td><td>{appointmentTime:HH:mm}</td></tr>
                <tr><td class='label'>Lý do</td><td>{reason}</td></tr>
            </table>
            
            <p>Vui lòng đặt lại lịch hẹn mới hoặc liên hệ phòng khám để được hỗ trợ.</p>
            
            <center>
                <a href='{rescheduleUrl}' class='btn'>ĐẶT LẠI LỊCH HẸN</a>
            </center>
        </div>
        <div class='footer'>
            <p><strong>{clinicName}</strong></p>
            <p>Chúng tôi xin lỗi vì sự bất tiện này.</p>
        </div>
    </div>
</body>
</html>";

            await _email
                .To(email)
                .Subject($"⚠️ Thông báo thay đổi lịch hẹn - {clinicName}")
                .Body(htmlBody, isHtml: true)
                .SendAsync();
        }

        public async Task<IResult> UpdateDoctorTimeOffAsync(Guid timeOffId, AddDoctorTimeOffRequest request)
        {
            var affectedRows = await _context.DoctorTimeOffs.Where(x => x.TimeOffId == timeOffId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(a => a.ClinicId, request.ClinicId)
                    .SetProperty(a => a.DoctorId, request.DoctorId)
                    .SetProperty(a => a.StartAt, request.StartAt)
                    .SetProperty(a => a.EndAt, request.EndAt)
                    .SetProperty(a => a.Reason, request.Reason)
                );
            return affectedRows > 0
                ? Results.Ok(
                    new ApiResponse<object>(true, $"Doctor time off updated successfully {affectedRows}", null))
                : Results.NoContent();
        }

        public async Task<IResult> DeleteDoctorTimeOffAsync(Guid timeOffId)
        {
            var affectedRows = await _context.DoctorTimeOffs.Where(x => x.TimeOffId == timeOffId).ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }

        public async Task<IResult> CreateDoctorAsync(CreateDoctorRequest request)
        {
            var existingDoctor = await _context.Doctors.AsNoTracking()
                .AnyAsync(d => d.Code == request.Code && d.ClinicId == request.ClinicId);
            if (existingDoctor)
            {
                return Results.Conflict(new ApiResponse<object>(false,
                    $"Doctor with code '{request.Code}' already exists in this clinic.", null));
            }

            var doctor = new Doctor
            {
                ClinicId = request.ClinicId,
                Code = request.Code,
                FullName = request.FullName,
                Specialty = request.Specialty,
                Phone = request.Phone,
                Email = request.Email,
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var doctorDto = new DoctorDto(doctor.DoctorId, doctor.ClinicId, doctor.Code, doctor.FullName,
                doctor.Specialty, doctor.Phone, doctor.Email, doctor.IsActive);
            return Results.Created($"/doctors/{doctor.DoctorId}",
                new ApiResponse<DoctorDto>(true, "Doctor created successfully", doctorDto));
        }

        public async Task<IResult> GetAllDoctorAsync()
        {
            var doctors = await _context.Doctors.AsNoTracking()
                .Select(doctor => new DoctorDto(doctor.DoctorId, doctor.ClinicId, doctor.Code, doctor.FullName,
                    doctor.Specialty, doctor.Phone, doctor.Email, doctor.IsActive))
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<DoctorDto>>(true, "Doctor retrieved successfully", doctors));
        }

        public async Task<IResult> UpdateDoctorAsync(Guid doctorId, UpdateDoctorRequest request)
        {
            var affectedRows = await _context.Doctors.AsNoTracking().Where(x => x.DoctorId == doctorId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(a => a.FullName, request.FullName)
                    .SetProperty(a => a.Specialty, request.Specialty)
                    .SetProperty(a => a.Phone, request.Phone)
                    .SetProperty(a => a.Email, request.Email)
                );
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Doctor updated successfully {affectedRows}", null))
                : Results.NoContent();
        }

        public async Task<IResult> DeleteDoctorAsync(Guid doctorId)
        {
            var affectedRows = await _context.Doctors.AsNoTracking().Where(x => x.DoctorId == doctorId)
                .ExecuteDeleteAsync();
            return affectedRows > 0
                ? Results.Ok(new ApiResponse<object>(true, $"Deleted {affectedRows} row(s)", null))
                : Results.NoContent();
        }
    }
}