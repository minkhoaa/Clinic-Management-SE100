using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.booking_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.booking_service.service
{
    public interface IUserService
    {
        Task<IResult> GetClinicsAsync(string? nameOrCode);
        Task<IResult> GetServicesAsync(Guid? clinicId, string? nameOrCode, bool? isActive);
        Task<IResult> GetDoctorsAsync(Guid? clinicId, string? nameOrCode, string? specialty, Guid? serviceId, bool? isActive);
        Task<IResult> GetAvailabilityAsync(Guid doctorId, DateOnly from, DateOnly to);
        Task<IResult> CreateAvailabilityAsync(CreateDoctorAvailability request);
        Task<IResult> UpdateAvailability(Guid availId, UpdateDoctorAvailability request);
        Task<IResult> GetSlotsAsync(Guid clinicId, Guid doctorId, Guid? serviceId, DateOnly date);
        Task<IResult> CreateBookingAsync(CreateBookingRequest req);
        Task<IResult> GetBookingAsync(Guid bookingId);
        Task<IResult> ConfirmBookingAsync(Guid bookingId);
        Task<IResult> CancelAppointmentAsync(string token);
        Task<IResult> ReschedulingAppointmentAsync(string token, DateTime startTime, DateTime startEnd);

    }

    public class UserService : IUserService
    {
        private readonly ClinicDbContext _context;
        public UserService(ClinicDbContext context)
        {
            _context = context;
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
                .Select(x => new ServiceDto(x.ServiceId, x.Code, x.Name, x.DefaultDurationMin, x.DefaultPrice, x.IsActive, x.ClinicId))
                .ToListAsync();

            return Results.Ok(new ApiResponse<IEnumerable<ServiceDto>>(true, "OK", data));
        }

        public async Task<IResult> GetDoctorsAsync(Guid? clinicId, string? nameOrCode, string? specialty, Guid? serviceId, bool? isActive)
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
                .Select(x => new DoctorDto(x.DoctorId, x.ClinicId, x.Code, x.FullName, x.Specialty, x.Phone, x.Email, x.IsActive))
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
                    && (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= date.ToDateTime(TimeOnly.MinValue))
                    && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= date.ToDateTime(TimeOnly.MinValue)))
                    ;

                results.AddRange(dayAvail.Select(x => new AvailabilityDto(date, x.StartTime, x.EndTime, x.SlotSizeMin)));
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
                .Where(x => x.DoctorId == doctorId && x.ClinicId == clinicId && x.IsActive && x.DayOfWeek == (byte)date.DayOfWeek)
                .ToListAsync();

            availabilities = availabilities.Where(x =>
                (!x.EffectiveFrom.HasValue || x.EffectiveFrom.Value.Date <= date.ToDateTime(TimeOnly.MinValue)) &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= date.ToDateTime(TimeOnly.MinValue))
            ).ToList();

            var doctorTimeOffs = await _context.DoctorTimeOffs
                .Where(t => t.DoctorId == doctorId && t.StartAt.Date <= date.ToDateTime(TimeOnly.MinValue) && t.EndAt.Date >= date.ToDateTime(TimeOnly.MinValue))
                .Select(t => new { t.StartAt, t.EndAt })
                .ToListAsync();

            var bookedAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.ClinicId == clinicId && a.StartAt.Date == date.ToDateTime(TimeOnly.MinValue).Date
                    && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
                .Select(a => new { a.StartAt, a.EndAt })
                .ToListAsync();

            var bookedPending = await _context.Bookings
                .Where(b => b.DoctorId == doctorId && b.ClinicId == clinicId && b.StartAt.Date == date.ToDateTime(TimeOnly.MinValue).Date
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
                .Select(b => new { b.StartAt, b.EndAt })
                .ToListAsync();

            var blocked = bookedAppointments.Concat(bookedPending).Concat(doctorTimeOffs).ToList();

            var result = new List<SlotDto>();
            foreach (var avail in availabilities)
            {
                var slotStart = date.ToDateTime(TimeOnly.FromTimeSpan(avail.StartTime));
                var end = date.ToDateTime(TimeOnly.FromTimeSpan(avail.EndTime));
                var size = TimeSpan.FromMinutes(avail.SlotSizeMin);

                while (slotStart + size <= end)
                {
                    var slotEnd = slotStart + size;
                    if (!blocked.Any(b => Overlaps(slotStart, slotEnd, b.StartAt, b.EndAt)))
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
                return Results.BadRequest(new ApiResponse<BookingResponse>(false, "Clinic not found", null));

            var doctor = await _context.Doctors.FindAsync(req.DoctorId);
            if (doctor is null || doctor.ClinicId != req.ClinicId)
                return Results.BadRequest(new ApiResponse<BookingResponse>(false, "Doctor not found", null));

            if (req.ServiceId.HasValue)
            {
                var serviceExists = await _context.Services.AnyAsync(x => x.ServiceId == req.ServiceId && x.ClinicId == req.ClinicId);
                if (!serviceExists)
                    return Results.BadRequest(new ApiResponse<BookingResponse>(false, "Service not found", null));

                var doctorSupportsService = await _context.DoctorServices.AnyAsync(ds =>
                    ds.DoctorId == req.DoctorId && ds.ServiceId == req.ServiceId && ds.IsEnabled);

                if (!doctorSupportsService)
                    return Results.BadRequest(new ApiResponse<BookingResponse>(false, "Doctor does not offer this service", null));
            }
            
            var hasTimeOffConflict = await _context.DoctorTimeOffs.AnyAsync(t =>
                t.DoctorId == req.DoctorId &&
                Overlaps(t.StartAt, t.EndAt, req.StartAt, req.EndAt));

            if (hasTimeOffConflict)
                return Results.Conflict(new ApiResponse<BookingResponse>(false, "Doctor is on time off during the selected period.", null));

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
                return Results.UnprocessableEntity(new ApiResponse<BookingResponse>(false, "Selected time is outside availability", null));

            // Conflict check
            var hasConflict = await _context.Appointments.AnyAsync(a =>
                a.ClinicId == req.ClinicId &&
                a.DoctorId == req.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                Overlaps(a.StartAt, a.EndAt, req.StartAt, req.EndAt));

            if (!hasConflict)
            {
                hasConflict = await _context.Bookings.AnyAsync(b =>
                    b.ClinicId == req.ClinicId &&
                    b.DoctorId == req.DoctorId &&
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                    Overlaps(b.StartAt, b.EndAt, req.StartAt, req.EndAt));
            }

            if (hasConflict)
                return Results.Conflict(new ApiResponse<BookingResponse>(false, "Slot already taken", null));

            var channel = req.Channel ?? AppointmentSource.Web;

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                ClinicId = req.ClinicId,
                DoctorId = req.DoctorId,
                ServiceId = req.ServiceId,
                StartAt = req.StartAt,
                EndAt = req.EndAt,
                FullName = req.FullName,
                Phone = req.Phone,
                Email = req.Email,
                Notes = req.Notes,
                Channel = channel,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var cancelToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var reschedulingToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var cancel = new BookingToken
            {
                BookingId = booking.BookingId,
                Action = "Cancel",
                Token = cancelToken,
                ExpiresAt = booking.StartAt
            };
            var reschedule = new BookingToken()
            {
                BookingId = booking.BookingId,
                Action = "Reschedule",
                Token = reschedulingToken,
                ExpiresAt = booking.StartAt
            };


            booking.Tokens.Add(cancel);
            booking.Tokens.Add(reschedule);

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Results.Created($"/bookings/{booking.BookingId}", new ApiResponse<BookingResponse>(true, "Created", new BookingResponse(booking.BookingId, booking.Status, cancelToken, null)));
        }

        public async Task<IResult> GetBookingAsync(Guid bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tokens)
                .FirstOrDefaultAsync(x => x.BookingId == bookingId);

            if (booking == null) return Results.NotFound(new ApiResponse<BookingResponse>(false, "Not found", null));

            var cancelToken = booking.Tokens.FirstOrDefault(t => t.Action == "Cancel")?.Token;
            var rescheduleToken = booking.Tokens.FirstOrDefault(t => t.Action == "Reschedule")?.Token;

            return Results.Ok(new ApiResponse<BookingResponse>(true, "OK", new BookingResponse(booking.BookingId, booking.Status, cancelToken, rescheduleToken)));
        }

        public async Task<IResult> ConfirmBookingAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(x => x.BookingId == bookingId);
            if (booking == null)
                return Results.NotFound(new ApiResponse<AppointmentResponse>(false, "Not found", null));

            if (booking.Status != BookingStatus.Pending)
                return Results.BadRequest(new ApiResponse<AppointmentResponse>(false, "Booking is not pending", null));

            var conflict = await _context.Appointments.AnyAsync(a =>
                a.ClinicId == booking.ClinicId &&
                a.DoctorId == booking.DoctorId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.NoShow &&
                Overlaps(a.StartAt, a.EndAt, booking.StartAt, booking.EndAt));

            if (!conflict)
            {
                conflict = await _context.Bookings.AnyAsync(b =>
                    b.BookingId != booking.BookingId &&
                    b.ClinicId == booking.ClinicId &&
                    b.DoctorId == booking.DoctorId &&
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                    Overlaps(b.StartAt, b.EndAt, booking.StartAt, booking.EndAt));
            }

            if (conflict)
                return Results.Conflict(new ApiResponse<AppointmentResponse>(false, "Slot already taken", null));

            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClinicId = booking.ClinicId,
                DoctorId = booking.DoctorId ?? throw new InvalidOperationException("Booking missing doctor"),
                ServiceId = booking.ServiceId,
                StartAt = booking.StartAt,
                EndAt = booking.EndAt,
                Source = booking.Channel,
                ContactFullName = booking.FullName,
                ContactPhone = booking.Phone,
                ContactEmail = booking.Email,
                Status = AppointmentStatus.Confirmed,
                BookingId = booking.BookingId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            booking.Status = BookingStatus.Confirmed;
            booking.Appointment = appointment;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Results.Created($"/appointments/{appointment.AppointmentId}", new ApiResponse<AppointmentResponse>(true, "Created", new AppointmentResponse(appointment.AppointmentId, appointment.Status)));
        }


        public async Task<IResult> ReschedulingAppointmentAsync(string token, DateTime startTime, DateTime endTime)
        {
            var reschedulingRequest = await _context.BookingTokens.Where(x => x.Token == token && x.Action == "Reschedule"
                                                                              && x.ExpiresAt > DateTime.UtcNow)
                                          .Include(bookingToken => bookingToken.Booking)
                                          .ThenInclude(booking => booking.Appointment)
                                          .FirstOrDefaultAsync() ??
                                      throw new Exception("Cannot found reschedule request");
            var booking = reschedulingRequest.Booking;
            var appointment = booking.Appointment ?? throw new Exception("Cannot find appointment");
            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                return Results.Conflict("Cannot rescheduling appointment");

            // Check overlap với các appointment khác (loại trừ chính appointment này và những cái đã cancel/noshow)
            var overlapAppointment = await _context.Appointments.AsNoTracking()
                .AnyAsync(x => x.AppointmentId != appointment.AppointmentId
                    && x.DoctorId == appointment.DoctorId
                    && x.Status != AppointmentStatus.Cancelled
                    && x.Status != AppointmentStatus.NoShow
                    && Overlaps(x.StartAt, x.EndAt, startTime, endTime));
            
            if (overlapAppointment) 
                return Results.Conflict("Appointment is conflicted");
            await using var transaction = await _context.Database.BeginTransactionAsync();
            appointment.StartAt = startTime;
            appointment.EndAt = endTime;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.Status = AppointmentStatus.Rescheduling;
            reschedulingRequest.ExpiresAt = DateTime.UtcNow;
            _context.Appointments.Update(appointment);
            _context.BookingTokens.Update(reschedulingRequest);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Ok(new
            {
                isSuccess = true,
                message = "Update successfully"
            });
        }


        public async Task<IResult> CancelAppointmentAsync(string token)
        {
            var cancelRequest = await _context.BookingTokens
                                    .Where(x => x.Token == token && x.Action == "Cancel" && x.ExpiresAt > DateTime.UtcNow)
                                    .Include(bookingToken => bookingToken.Booking)
                                    .ThenInclude(booking => booking.Appointment).FirstOrDefaultAsync()
                                ?? throw new Exception("Not found cancel request");
            var booking = cancelRequest.Booking;
            var appointment = booking.Appointment ?? throw new Exception("Cannot get appointment information");
            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                return Results.Conflict("Cannot cancel cancelled appointment ");
            int cutoffHours = 2;
            if (appointment.StartAt < DateTime.UtcNow.AddHours(cutoffHours))
                return Results.Conflict("Cannot cancel appointment within 2 hours");
            await using var transaction = await _context.Database.BeginTransactionAsync();
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;
            _context.Appointments.Update(appointment);
            cancelRequest.ExpiresAt = DateTime.UtcNow;
            _context.BookingTokens.Update(cancelRequest);


            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Ok(new
            {
                bookingId = booking.BookingId,
                appointmentId = appointment.AppointmentId,
                status = appointment.Status
            });
        }



        private static bool Overlaps(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
            => start1 < end2 && start2 < end1;
    }

}
