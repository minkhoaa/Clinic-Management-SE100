using ClinicManagement_API.Contracts;
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
        Task<IResult> UpdateAppointmentStatusAsync(Guid id, UpdateAppointmentStatusRequest request);
        Task<IResult> GetTimeSlotsAsync(Guid clinicId, Guid doctorId, DateOnly date);

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
                PatientId = req.PatientId,
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
            var reschedulingRequest = await _context.BookingTokens
                .Where(x => x.Token == token && x.Action == "Reschedule" && x.ExpiresAt > DateTime.UtcNow)
                .Include(bookingToken => bookingToken.Booking)
                .ThenInclude(booking => booking.Appointment)
                .FirstOrDefaultAsync();
            
            if (reschedulingRequest == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Reschedule token not found or expired", null));
            }
            
            var booking = reschedulingRequest.Booking;
            
            // Check if booking has an appointment (confirmed)
            if (booking.Appointment != null)
            {
                var appointment = booking.Appointment;
                
                if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                    return Results.Conflict(new ApiResponse<object>(false, "Cannot reschedule cancelled appointment", null));

                // Check overlap with other appointments
                var overlapAppointment = await _context.Appointments.AsNoTracking()
                    .AnyAsync(x => x.AppointmentId != appointment.AppointmentId
                        && x.DoctorId == appointment.DoctorId
                        && x.Status != AppointmentStatus.Cancelled
                        && x.Status != AppointmentStatus.NoShow
                        && Overlaps(x.StartAt, x.EndAt, startTime, endTime));

                if (overlapAppointment)
                    return Results.Conflict(new ApiResponse<object>(false, "Time slot is not available", null));
                
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                appointment.StartAt = startTime;
                appointment.EndAt = endTime;
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.Status = AppointmentStatus.Rescheduling;
                booking.StartAt = startTime;
                booking.EndAt = endTime;
                booking.UpdatedAt = DateTime.UtcNow;
                reschedulingRequest.ExpiresAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return Results.Ok(new ApiResponse<object>(true, "Appointment rescheduled successfully", new
                {
                    bookingId = booking.BookingId,
                    appointmentId = appointment.AppointmentId,
                    newStartAt = startTime,
                    newEndAt = endTime
                }));
            }
            else
            {
                // Booking not yet confirmed - just update the booking time
                if (booking.Status == BookingStatus.Cancelled)
                    return Results.Conflict(new ApiResponse<object>(false, "Cannot reschedule cancelled booking", null));
                
                // Check overlap with other bookings
                var overlapBooking = await _context.Bookings.AsNoTracking()
                    .AnyAsync(x => x.BookingId != booking.BookingId
                        && x.DoctorId == booking.DoctorId
                        && (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Confirmed)
                        && Overlaps(x.StartAt, x.EndAt, startTime, endTime));
                
                if (overlapBooking)
                    return Results.Conflict(new ApiResponse<object>(false, "Time slot is not available", null));
                
                booking.StartAt = startTime;
                booking.EndAt = endTime;
                booking.UpdatedAt = DateTime.UtcNow;
                reschedulingRequest.ExpiresAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                return Results.Ok(new ApiResponse<object>(true, "Booking rescheduled successfully", new
                {
                    bookingId = booking.BookingId,
                    newStartAt = startTime,
                    newEndAt = endTime
                }));
            }
        }


        public async Task<IResult> CancelAppointmentAsync(string token)
        {
            var cancelRequest = await _context.BookingTokens
                                    .Where(x => x.Token == token && x.Action == "Cancel" && x.ExpiresAt > DateTime.UtcNow)
                                    .Include(bookingToken => bookingToken.Booking)
                                    .ThenInclude(booking => booking.Appointment).FirstOrDefaultAsync();
            
            if (cancelRequest == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Cancel token not found or expired", null));
            }
            
            var booking = cancelRequest.Booking;
            int cutoffHours = 2;
            
            // Check if booking has an appointment (confirmed)
            if (booking.Appointment != null)
            {
                var appointment = booking.Appointment;
                
                if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
                    return Results.Conflict(new ApiResponse<object>(false, "Appointment is already cancelled", null));
                
                if (appointment.StartAt < DateTime.UtcNow.AddHours(cutoffHours))
                    return Results.Conflict(new ApiResponse<object>(false, "Cannot cancel appointment within 2 hours of start time", null));
                
                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;
                cancelRequest.ExpiresAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return Results.Ok(new ApiResponse<object>(true, "Appointment cancelled successfully", new
                {
                    bookingId = booking.BookingId,
                    appointmentId = appointment.AppointmentId,
                    status = "cancelled"
                }));
            }
            else
            {
                // Booking not yet confirmed - just cancel the booking
                if (booking.Status == BookingStatus.Cancelled)
                    return Results.Conflict(new ApiResponse<object>(false, "Booking is already cancelled", null));
                
                if (booking.StartAt < DateTime.UtcNow.AddHours(cutoffHours))
                    return Results.Conflict(new ApiResponse<object>(false, "Cannot cancel booking within 2 hours of start time", null));
                
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;
                cancelRequest.ExpiresAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                return Results.Ok(new ApiResponse<object>(true, "Booking cancelled successfully", new
                {
                    bookingId = booking.BookingId,
                    status = "cancelled"
                }));
            }
        }



        private static bool Overlaps(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
            => start1 < end2 && start2 < end1;

        public async Task<IResult> UpdateAppointmentStatusAsync(Guid id, UpdateAppointmentStatusRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.Appointment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return Results.NotFound(new ApiResponse<object>(false, "Booking not found", null));
            }

            // Parse the status
            if (!Enum.TryParse<BookingStatus>(request.Status, true, out var newStatus))
            {
                return Results.BadRequest(new ApiResponse<object>(false, "Invalid status. Use: confirmed or cancelled", null));
            }

            // Validate status transitions
            if (newStatus != BookingStatus.Confirmed && newStatus != BookingStatus.Cancelled)
            {
                return Results.BadRequest(new ApiResponse<object>(false, "Only 'confirmed' or 'cancelled' status is allowed", null));
            }

            if (booking.Status == newStatus)
            {
                return Results.BadRequest(new ApiResponse<object>(false, $"Booking is already {newStatus.ToString().ToLower()}", null));
            }

            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            // If confirming, create appointment if not exists
            if (newStatus == BookingStatus.Confirmed && booking.Appointment == null)
            {
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
                booking.Appointment = appointment;
                _context.Appointments.Add(appointment);
            }

            // If cancelling, cancel appointment too
            if (newStatus == BookingStatus.Cancelled && booking.Appointment != null)
            {
                booking.Appointment.Status = AppointmentStatus.Cancelled;
                booking.Appointment.UpdatedAt = DateTime.UtcNow;
            }

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
