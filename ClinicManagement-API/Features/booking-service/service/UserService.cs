using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    public interface IUserService
    {
        Task<IResult> GetClinicsAsync(string? nameOrCode);
        Task<IResult> GetServicesAsync(Guid? clinicId, string? nameOrCode, bool? isActive);

        Task<IResult> GetDoctorsAsync(Guid? clinicId, string? nameOrCode, string? specialty, Guid? serviceId,
            bool? isActive);

        Task<IResult> GetAvailabilityAsync(Guid doctorId, DateOnly from, DateOnly to);
        Task<IResult> CreateAvailabilityAsync(CreateDoctorAvailability request);
        Task<IResult> UpdateAvailability(Guid availId, UpdateDoctorAvailability request);
        Task<IResult> GetSlotsAsync(Guid clinicId, Guid doctorId, Guid? serviceId, DateOnly date);
        Task<IResult> CreateBookingAsync(CreateBookingRequest req);
        Task<IResult> GetAppointmentAsync(Guid appointmentId);
        Task<IResult> ConfirmAppointmentAsync(Guid appointmentId);
        Task<IResult> CancelAppointmentAsync(string token);
        Task<IResult> ReschedulingAppointmentAsync(string token, DateTime startTime, DateTime startEnd);
        Task<IResult> UpdateAppointmentStatusAsync(Guid id, UpdateAppointmentStatusRequest request);
        Task<IResult> GetTimeSlotsAsync(Guid clinicId, Guid doctorId, DateOnly date);
    }

    public class UserService : IUserService
    {
        private readonly ClinicDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserService(ClinicDbContext context, UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IResult> GetClinicsAsync(string? nameOrCode)
        {
            var query = _context.Clinics.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(nameOrCode))
            {
                query = query.Where(x => x.Name.Contains(nameOrCode) || x.Code.Contains(nameOrCode));
            }

            var data = await query
                .OrderBy(x => x.Name)
                .Select(x => new ClinicDto(x.ClinicId, x.Code, x.Name, x.TimeZone, x.Phone, x.Email))
                .ToListAsync();

            return Results.Ok(new ApiResponse<IEnumerable<ClinicDto>>(true, "OK", data));
        }

        public async Task<IResult> GetServicesAsync(Guid? clinicId, string? nameOrCode, bool? isActive)
        {
            var query = _context.Services.AsNoTracking();

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId);

            if (!string.IsNullOrWhiteSpace(nameOrCode))
                query = query.Where(x => x.Name.Contains(nameOrCode) || x.Code.Contains(nameOrCode));

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var data = await query
                .OrderBy(x => x.Name)
                .Select(x => new ServiceDto(x.ServiceId, x.Code, x.Name, x.DefaultDurationMin, x.DefaultPrice,
                    x.IsActive, x.ClinicId))
                .ToListAsync();

            return Results.Ok(new ApiResponse<IEnumerable<ServiceDto>>(true, "OK", data));
        }

        public async Task<IResult> GetDoctorsAsync(Guid? clinicId, string? nameOrCode, string? specialty,
            Guid? serviceId, bool? isActive)
        {
            var query = _context.Doctors.AsNoTracking();

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId);

            if (!string.IsNullOrWhiteSpace(nameOrCode))
                query = query.Where(x => x.FullName.Contains(nameOrCode) || x.Code.Contains(nameOrCode));

            if (!string.IsNullOrWhiteSpace(specialty))
                query = query.Where(x => x.Specialty != null && x.Specialty.Contains(specialty));

            if (serviceId.HasValue)
            {
                query = query.Where(x => x.DoctorServices.Any(ds => ds.ServiceId == serviceId && ds.IsEnabled));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var data = await query
                .OrderBy(x => x.FullName)
                .Select(x => new DoctorDto(x.DoctorId, x.ClinicId, x.Code, x.FullName, x.Specialty, x.Phone, x.Email,
                    x.IsActive))
                .ToListAsync();

            return Results.Ok(new ApiResponse<IEnumerable<DoctorDto>>(true, "OK", data));
        }

        public async Task<IResult> GetAvailabilityAsync(Guid doctorId, DateOnly from, DateOnly to)
        {
            var availabilities = await _context.DoctorAvailabilities
                .Where(x => x.DoctorId == doctorId && x.IsActive)
                .ToListAsync();

            var results = new List<AvailabilityDto>();
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var dow = (byte)date.DayOfWeek;
                var dayAvail = availabilities.Where(x => x.DayOfWeek == dow
                                                         && (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <=
                                                             date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                                                         && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >=
                                                             date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)))
                    ;

                results.AddRange(dayAvail.Select(x =>
                    new AvailabilityDto(date, x.StartTime, x.EndTime, x.SlotSizeMin)));
            }

            return Results.Ok(new ApiResponse<IEnumerable<AvailabilityDto>>(true, "OK", results));
        }

        public async Task<IResult> CreateAvailabilityAsync(CreateDoctorAvailability request)
        {
            var existedClinic = await _context.Clinics.AsNoTracking().AnyAsync(a => a.ClinicId == request.ClinicId);
            if (!existedClinic) return Results.NotFound("Clinic is not found");
            var existedDoctor = await _context.Doctors.AsNoTracking().AnyAsync(a => a.DoctorId == request.DoctorId);
            if (!existedDoctor) return Results.NotFound("Doctor is not found");
            var aval = new DoctorAvailability
            {
                DoctorId = request.DoctorId,
                ClinicId = request.ClinicId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = request.IsActive,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                SlotSizeMin = request.SlotSizeMin
            };
            _context.DoctorAvailabilities.Add(aval);
            await _context.SaveChangesAsync();
            return Results.Ok(new ApiResponse<IEnumerable<AvailabilityDto>>(true, "OK", null));
        }

        public async Task<IResult> UpdateAvailability(Guid availId, UpdateDoctorAvailability request)
        {
            var existedAval = await _context.DoctorAvailabilities
                                  .FirstOrDefaultAsync(x => x.AvailabilityId == availId) ??
                              throw new Exception("Cannot found Availability");
            existedAval.EndTime = request.EndTime;
            existedAval.IsActive = request.IsActive;
            existedAval.SlotSizeMin = request.SlotSizeMin;
            _context.DoctorAvailabilities.Update(existedAval);
            await _context.SaveChangesAsync();
            return Results.Ok($"Updated {existedAval.AvailabilityId}");
        }

        public async Task<IResult> GetSlotsAsync(Guid clinicId, Guid doctorId, Guid? serviceId, DateOnly date)
        {
            var availabilities = await _context.DoctorAvailabilities
                .Where(x => x.DoctorId == doctorId && x.ClinicId == clinicId && x.IsActive &&
                            x.DayOfWeek == (byte)date.DayOfWeek)
                .ToListAsync();

            availabilities = availabilities.Where(x =>
                (!x.EffectiveFrom.HasValue ||
                 x.EffectiveFrom.Value.Date <= date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)) &&
                (!x.EffectiveTo.HasValue ||
                 x.EffectiveTo.Value.Date >= date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            ).ToList();

            var dateUtc = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var doctorTimeOffs = await _context.DoctorTimeOffs
                .Where(t => t.DoctorId == doctorId && t.StartAt.Date <= dateUtc && t.EndAt.Date >= dateUtc)
                .Select(t => new { t.StartAt, t.EndAt })
                .ToListAsync();

            var bookedAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.ClinicId == clinicId && a.StartAt.Date == dateUtc.Date
                            && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
                .Select(a => new { a.StartAt, a.EndAt })
                .ToListAsync();

            var blocked = bookedAppointments.Concat(doctorTimeOffs).ToList();

            var result = new List<SlotDto>();
            foreach (var avail in availabilities)
            {
                var slotStart = date.ToDateTime(TimeOnly.FromTimeSpan(avail.StartTime), DateTimeKind.Utc);
                var end = date.ToDateTime(TimeOnly.FromTimeSpan(avail.EndTime), DateTimeKind.Utc);
                var size = TimeSpan.FromMinutes(avail.SlotSizeMin);

                while (slotStart + size <= end)
                {
                    var slotEnd = slotStart + size;
                    if (!blocked.Any(b => slotStart < b.EndAt && b.StartAt < slotEnd))
                    {
                        result.Add(new SlotDto(slotStart, slotEnd));
                    }

                    slotStart = slotEnd;
                }
            }

            return Results.Ok(new ApiResponse<IEnumerable<SlotDto>>(true, "OK", result.OrderBy(x => x.StartAt)));
        }

        public async Task<IResult> CreateBookingAsync(CreateBookingRequest req)
        {
            var clinic = await _context.Clinics.FindAsync(req.ClinicId);
            if (clinic is null)
                return Results.BadRequest(new ApiResponse<AppointmentResponse>(false, "Clinic not found", null));

            var doctor = await _context.Doctors.FindAsync(req.DoctorId);
            if (doctor is null || doctor.ClinicId != req.ClinicId)
                return Results.BadRequest(new ApiResponse<AppointmentResponse>(false, "Doctor not found", null));

            if (req.ServiceId.HasValue)
            {
                var serviceExists =
                    await _context.Services.AnyAsync(x => x.ServiceId == req.ServiceId && x.ClinicId == req.ClinicId);
                if (!serviceExists)
                    return Results.BadRequest(new ApiResponse<AppointmentResponse>(false, "Service not found", null));

                var doctorSupportsService = await _context.DoctorServices.AnyAsync(ds =>
                    ds.DoctorId == req.DoctorId && ds.ServiceId == req.ServiceId && ds.IsEnabled);

                if (!doctorSupportsService)
                    return Results.BadRequest(
                        new ApiResponse<AppointmentResponse>(false, "Doctor does not offer this service", null));
            }

            var hasTimeOffConflict = await _context.DoctorTimeOffs.AnyAsync(t =>
                t.DoctorId == req.DoctorId &&
                (t.StartAt < req.EndAt && req.StartAt < t.EndAt));

            if (hasTimeOffConflict)
                return Results.Conflict(new ApiResponse<AppointmentResponse>(false,
                    "Doctor is on time off during the selected period.", null));

            // Validate slot inside availability
            var date = DateOnly.FromDateTime(req.StartAt);
            var avail = await _context.DoctorAvailabilities
                .Where(x => x.DoctorId == req.DoctorId && x.ClinicId == req.ClinicId
                                                       && x.IsActive && x.DayOfWeek == (byte)date.DayOfWeek)
                .ToListAsync();

            var inAvail = avail.Any(x =>
                (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= req.StartAt.Date) &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= req.StartAt.Date) &&
                req.StartAt.TimeOfDay >= x.StartTime &&
                req.EndAt.TimeOfDay <= x.EndTime);

            if (!inAvail)
                return Results.UnprocessableEntity(
                    new ApiResponse<AppointmentResponse>(false, "Selected time is outside availability", null));

            var hasConflict = await _context.Appointments.AnyAsync(a =>
                a.ClinicId == req.ClinicId &&
                a.DoctorId == req.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                (a.StartAt < req.EndAt && req.StartAt < a.EndAt));

            if (hasConflict)
                return Results.Conflict(new ApiResponse<AppointmentResponse>(false, "Slot already taken", null));

            var source = req.Channel ?? AppointmentSource.Web;

            // PatientId will be created on confirm - for now just use if provided
            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClinicId = req.ClinicId,
                DoctorId = req.DoctorId,
                ServiceId = req.ServiceId,
                PatientId = req.PatientId, // Only set if already exists
                StartAt = req.StartAt,
                EndAt = req.EndAt,
                ContactFullName = req.FullName,
                ContactPhone = req.Phone,
                ContactEmail = req.Email,
                Notes = req.Notes,
                Source = source,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var cancelToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var reschedulingToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var cancel = new AppointmentToken
            {
                AppointmentId = appointment.AppointmentId,
                Action = "Cancel",
                Token = cancelToken,
                ExpiresAt = appointment.StartAt
            };
            var reschedule = new AppointmentToken()
            {
                AppointmentId = appointment.AppointmentId,
                Action = "Reschedule",
                Token = reschedulingToken,
                ExpiresAt = appointment.StartAt
            };

            appointment.Tokens.Add(cancel);
            appointment.Tokens.Add(reschedule);

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Results.Created($"/appointments/{appointment.AppointmentId}",
                new ApiResponse<AppointmentResponse>(true, "Created",
                    new AppointmentResponse(appointment.AppointmentId, appointment.PatientId, appointment.Status,
                        cancelToken, reschedulingToken, null, null)));
        }

        public async Task<IResult> GetAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Tokens)
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (appointment == null)
                return Results.NotFound(new ApiResponse<AppointmentResponse>(false, "Not found", null));

            var cancelToken = appointment.Tokens.FirstOrDefault(t => t.Action == "Cancel")?.Token;
            var rescheduleToken = appointment.Tokens.FirstOrDefault(t => t.Action == "Reschedule")?.Token;

            return Results.Ok(new ApiResponse<AppointmentResponse>(true, "OK",
                new AppointmentResponse(appointment.AppointmentId, appointment.PatientId, appointment.Status,
                    cancelToken, rescheduleToken, null, null)));
        }

        public async Task<IResult> ConfirmAppointmentAsync(Guid appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
            if (appointment == null)
                return Results.NotFound(new ApiResponse<AppointmentResponse>(false, "Not found", null));

            if (appointment.Status != AppointmentStatus.Pending)
                return Results.BadRequest(
                    new ApiResponse<AppointmentResponse>(false, "Appointment is not pending", null));

            // Check for conflicts with other confirmed appointments
            var conflict = await _context.Appointments.AnyAsync(a =>
                a.AppointmentId != appointmentId &&
                a.ClinicId == appointment.ClinicId &&
                a.DoctorId == appointment.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                a.Status != AppointmentStatus.Pending &&
                (a.StartAt < appointment.EndAt && appointment.StartAt < a.EndAt));

            if (conflict)
                return Results.Conflict(new ApiResponse<AppointmentResponse>(false,
                    "Slot already taken by confirmed appointment", null));

            // Create patient and user account on confirmation
            Guid? patientId = appointment.PatientId;
            string? createdUsername = null;
            string? createdPassword = null;

            if (!patientId.HasValue && !string.IsNullOrEmpty(appointment.ContactPhone))
            {
                // Track that we're creating a new user
                createdUsername = appointment.ContactPhone;
                createdPassword = appointment.ContactPhone;

                // Try to find existing patient by phone
                var existingPatient = await _context.Patients
                    .FirstOrDefaultAsync(p =>
                        p.ClinicId == appointment.ClinicId && p.PrimaryPhone == appointment.ContactPhone);

                if (existingPatient != null)
                {
                    patientId = existingPatient.PatientId;

                    // Link user if patient has no user
                    if (!existingPatient.UserId.HasValue)
                    {
                        var existingUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.PhoneNumber == appointment.ContactPhone);
                        if (existingUser == null)
                        {
                            existingUser = new User
                            {
                                Id = Guid.NewGuid(),
                                UserName = appointment.ContactPhone,
                                PhoneNumber = appointment.ContactPhone,
                                Email = appointment.ContactEmail,
                                PhoneNumberConfirmed = true,
                                SecurityStamp = Guid.NewGuid().ToString()
                            };
                            if (!await _roleManager.RoleExistsAsync(AppRoles.Patient))
                            {
                                await _roleManager.CreateAsync(new Role() { Name = AppRoles.Patient });
                            }

                            var createResult = await _userManager.CreateAsync(existingUser, appointment.ContactPhone);
                            if (createResult.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(existingUser, AppRoles.Patient);
                            }
                        }
                        else
                        {
                            // User already exists, no new credentials to return
                            createdUsername = null;
                            createdPassword = null;
                        }

                        existingPatient.UserId = existingUser.Id;
                        await _context.SaveChangesAsync(); // Save patient update immediately
                    }
                    else
                    {
                        // Patient already has user
                        createdUsername = null;
                        createdPassword = null;
                    }
                }
                else
                {
                    // Create new user first
                    var newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = appointment.ContactPhone,
                        PhoneNumber = appointment.ContactPhone,
                        Email = appointment.ContactEmail,
                        PhoneNumberConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    if (!await _roleManager.RoleExistsAsync(AppRoles.Patient))
                    {
                        await _roleManager.CreateAsync(new Role() { Name = AppRoles.Patient });
                    }

                    var createResult = await _userManager.CreateAsync(newUser, appointment.ContactPhone);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, AppRoles.Patient);
                    }

                    // Create patient first without UserId to avoid FK issues
                    var patientCount = await _context.Patients.CountAsync(p => p.ClinicId == appointment.ClinicId);
                    var newPatient = new Patients
                    {
                        PatientId = Guid.NewGuid(),
                        ClinicId = appointment.ClinicId,
                        PatientCode = $"BN{(patientCount + 1):D6}",
                        FullName = appointment.ContactFullName,
                        PrimaryPhone = appointment.ContactPhone,
                        Email = appointment.ContactEmail,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Patients.Add(newPatient);
                    await _context.SaveChangesAsync();

                    // Now update UserId after Patient is saved
                    if (createResult.Succeeded)
                    {
                        newPatient.UserId = newUser.Id;
                        await _context.SaveChangesAsync();
                    }

                    patientId = newPatient.PatientId;
                }

                appointment.PatientId = patientId;
            }

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cancelToken = appointment.Tokens.FirstOrDefault(t => t.Action == "Cancel")?.Token;
            var rescheduleToken = appointment.Tokens.FirstOrDefault(t => t.Action == "Reschedule")?.Token;

            return Results.Ok(new ApiResponse<AppointmentResponse>(true, "Appointment confirmed",
                new AppointmentResponse(appointment.AppointmentId, appointment.PatientId, appointment.Status,
                    cancelToken, rescheduleToken, createdUsername, createdPassword)));
        }


        public async Task<IResult> ReschedulingAppointmentAsync(string token, DateTime startTime, DateTime endTime)
        {
            var reschedulingRequest = await _context.AppointmentTokens
                .Where(x => x.Token == token && x.Action == "Reschedule" && x.ExpiresAt > DateTime.UtcNow)
                .Include(t => t.Appointment)
                .FirstOrDefaultAsync();

            if (reschedulingRequest == null)
                return Results.NotFound(new ApiResponse<object>(false, "Reschedule token not found or expired",
                    null));

            var appointment = reschedulingRequest.Appointment;

            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                return Results.Conflict(new ApiResponse<object>(false, "Cannot reschedule cancelled appointment",
                    null));

            // Check overlap with other appointments
            var hasConflict = await _context.Appointments.AsNoTracking()
                .AnyAsync(x => x.AppointmentId != appointment.AppointmentId
                               && x.DoctorId == appointment.DoctorId
                               && x.Status != AppointmentStatus.Cancelled
                               && x.Status != AppointmentStatus.NoShow
                               && (x.StartAt < endTime && startTime < x.EndAt));

            if (hasConflict)
                return Results.Conflict(new ApiResponse<object>(false, "Time slot is not available", null));

            appointment.StartAt = startTime;
            appointment.EndAt = endTime;
            appointment.UpdatedAt = DateTime.UtcNow;
            reschedulingRequest.ExpiresAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>(true, "Appointment rescheduled successfully", new
            {
                appointmentId = appointment.AppointmentId,
                newStartAt = startTime,
                newEndAt = endTime
            }));
        }


        public async Task<IResult> CancelAppointmentAsync(string token)
        {
            var cancelRequest = await _context.AppointmentTokens
                .Where(x => x.Token == token && x.Action == "Cancel" && x.ExpiresAt > DateTime.UtcNow)
                .Include(t => t.Appointment)
                .FirstOrDefaultAsync();

            if (cancelRequest == null)
                return Results.NotFound(new ApiResponse<object>(false, "Cancel token not found or expired", null));

            var appointment = cancelRequest.Appointment;
            int cutoffHours = 2;

            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                return Results.Conflict(new ApiResponse<object>(false, "Appointment is already cancelled", null));

            if (appointment.StartAt < DateTime.UtcNow.AddHours(cutoffHours))
                return Results.Conflict(new ApiResponse<object>(false,
                    "Cannot cancel appointment within 2 hours of start time", null));

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            cancelRequest.ExpiresAt = DateTime.UtcNow; // Invalidate token after use

            await _context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>(true, "Appointment cancelled successfully", new
            {
                appointmentId = appointment.AppointmentId,
                status = "cancelled"
            }));
        }


        public async Task<IResult> UpdateAppointmentStatusAsync(Guid id, UpdateAppointmentStatusRequest request)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));

            // Parse the status
            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatus))
                return Results.BadRequest(new ApiResponse<object>(false, "Invalid status", null));

            if (appointment.Status == newStatus)
                return Results.BadRequest(new ApiResponse<object>(false,
                    $"Appointment is already {newStatus.ToString().ToLower()}", null));

            appointment.Status = newStatus;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Results.Ok(new ApiResponse<object>(true, $"Status updated to {newStatus.ToString().ToLower()}",
                new { appointment_id = id, new_status = newStatus.ToString().ToLower() }));
        }

        public async Task<IResult> GetTimeSlotsAsync(Guid clinicId, Guid doctorId, DateOnly date)
        {
            // Get available slots using existing logic
            var slotsResult = await GetSlotsAsync(clinicId, doctorId, null, date);

            // Extract slots from the result
            if (slotsResult is Microsoft.AspNetCore.Http.HttpResults.Ok<ApiResponse<IEnumerable<SlotDto>>> okResult)
            {
                var slots = okResult.Value?.Data;
                if (slots != null)
                {
                    var timeSlots = slots.Select(s => s.StartAt.ToString("HH:mm")).ToList();
                    return Results.Ok(new ApiResponse<List<string>>(true, "OK", timeSlots));
                }
            }

            return Results.Ok(new ApiResponse<List<string>>(true, "OK", new List<string>()));
        }
    }
}
