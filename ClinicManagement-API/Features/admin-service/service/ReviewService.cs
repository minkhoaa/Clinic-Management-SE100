using ClinicManagement_API.Contracts;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.admin_service.dto;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.admin_service.service;

public interface IReviewService
{
    Task<IResult> CreateReviewAsync(Guid userId, CreateReviewRequest request);
    Task<IResult> GetReviewStatsAsync();
}

public class ReviewService : IReviewService
{
    private readonly ClinicDbContext _context;

    public ReviewService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<IResult> CreateReviewAsync(Guid userId, CreateReviewRequest request)
    {
        // Validate rating
        if (request.Rating < 1 || request.Rating > 5)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "Rating must be between 1 and 5", null));
        }

        // Find patient by userId (userId from token ≠ patientId)
        var patient = await _context.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (patient == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Patient profile not found", null));
        }

        // Check if appointment exists and belongs to patient
        var appointment = await _context.Appointments.AsNoTracking()
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId);

        if (appointment == null)
        {
            return Results.NotFound(new ApiResponse<object>(false, "Appointment not found", null));
        }

        // Use patient.PatientId (not userId) for comparison
        if (appointment.PatientId != patient.PatientId)
        {
            return Results.BadRequest(new ApiResponse<object>(false, "You can only review your own appointments",
                null));
        }

        // Check if appointment is completed
        if (appointment.Status != AppointmentStatus.Completed)
        {
            return Results.BadRequest(
                new ApiResponse<object>(false, "You can only review completed appointments", null));
        }

        // Check if already reviewed
        var existingReview = await _context.Reviews.AsNoTracking()
            .AnyAsync(r => r.AppointmentId == request.AppointmentId);

        if (existingReview)
        {
            return Results.BadRequest(
                new ApiResponse<object>(false, "This appointment has already been reviewed", null));
        }

        var review = new Review
        {
            AppointmentId = request.AppointmentId,
            PatientId = patient.PatientId, // Use patient.PatientId, not userId
            DoctorId = appointment.DoctorId,
            ClinicId = appointment.ClinicId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var result = new ReviewDto(
            review.ReviewId,
            review.AppointmentId,
            review.Rating,
            review.Comment,
            review.CreatedAt
        );

        return Results.Created($"/api/patient/reviews/{review.ReviewId}",
            new ApiResponse<ReviewDto>(true, "Review created successfully", result));
    }

    public async Task<IResult> GetReviewStatsAsync()
    {
        var reviews = await _context.Reviews.AsNoTracking().ToListAsync();

        if (reviews.Count == 0)
        {
            var emptyStats = new ReviewStatsDto(
                0,
                0,
                0,
                new RatingDistributionDto(0, 0, 0, 0, 0)
            );
            return Results.Ok(new ApiResponse<ReviewStatsDto>(true, "Review stats retrieved successfully", emptyStats));
        }

        var averageRating = Math.Round((decimal)reviews.Average(r => r.Rating), 2);
        var totalReviews = reviews.Count;
        var satisfiedCount = reviews.Count(r => r.Rating >= 4);
        var satisfactionRate = Math.Round((decimal)satisfiedCount / totalReviews * 100, 1);

        var ratingDistribution = new RatingDistributionDto(
            reviews.Count(r => r.Rating == 5),
            reviews.Count(r => r.Rating == 4),
            reviews.Count(r => r.Rating == 3),
            reviews.Count(r => r.Rating == 2),
            reviews.Count(r => r.Rating == 1)
        );

        var stats = new ReviewStatsDto(
            averageRating,
            totalReviews,
            satisfactionRate,
            ratingDistribution
        );

        return Results.Ok(new ApiResponse<ReviewStatsDto>(true, "Review stats retrieved successfully", stats));
    }
}
