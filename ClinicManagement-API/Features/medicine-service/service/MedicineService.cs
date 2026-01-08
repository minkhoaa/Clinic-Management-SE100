using System.Security.Claims;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Features.medicine_service.dto;
using ClinicManagement_API.Contracts;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Features.medicine_service.service;

public interface IMedicineService
{
    Task<IResult> GetMedicinesAsync(ClaimsPrincipal user);
    Task<IResult> CreateMedicineAsync(ClaimsPrincipal user, CreateMedicineRequest request);
    Task<IResult> UpdateMedicineAsync(ClaimsPrincipal user, Guid medicineId, UpdateMedicineRequest request);
    Task<IResult> DeleteMedicineAsync(ClaimsPrincipal user, Guid medicineId);
}

public class MedicineService : IMedicineService
{
    private readonly ClinicDbContext _context;

    public MedicineService(ClinicDbContext context)
    {
        _context = context;
    }

    private async Task<Guid?> GetClinicIdFromUser(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        // Get ClinicId from StaffUser (Doctor or Receptionist)
        var staffUser = await _context.StaffUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        return staffUser?.ClinicId;
    }

    public async Task<IResult> GetMedicinesAsync(ClaimsPrincipal user)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var medicines = await _context.Medicines
            .AsNoTracking()
            .Where(m => m.ClinicId == clinicId.Value)
            .OrderBy(m => m.Name)
            .Select(m => new MedicineListDto(
                m.MedicineId,
                m.Code,
                m.Name,
                m.Unit,
                m.Price,
                m.IsActive
            ))
            .ToListAsync();

        return Results.Ok(new ApiResponse<List<MedicineListDto>>(true, "Medicines retrieved", medicines));
    }

    public async Task<IResult> CreateMedicineAsync(ClaimsPrincipal user, CreateMedicineRequest request)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        // Check unique code within clinic
        var exists = await _context.Medicines
            .AnyAsync(m => m.ClinicId == clinicId.Value && m.Code == request.Code);
        if (exists)
            return Results.BadRequest(new ApiResponse<object>(false, "Medicine code already exists in this clinic",
                null));

        var medicine = new Medicine
        {
            MedicineId = Guid.NewGuid(),
            ClinicId = clinicId.Value,
            Code = request.Code,
            Name = request.Name,
            Unit = request.Unit,
            Price = request.Price,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        return Results.Created($"/api/medicines/{medicine.MedicineId}",
            new ApiResponse<CreateMedicineResponse>(true, "Medicine created",
                new CreateMedicineResponse(medicine.MedicineId, medicine.CreatedAt)));
    }

    public async Task<IResult> UpdateMedicineAsync(ClaimsPrincipal user, Guid medicineId, UpdateMedicineRequest request)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.MedicineId == medicineId && m.ClinicId == clinicId.Value);

        if (medicine == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medicine not found", null));

        if (request.Name != null) medicine.Name = request.Name;
        if (request.Unit != null) medicine.Unit = request.Unit;
        if (request.Price.HasValue) medicine.Price = request.Price.Value;
        if (request.Description != null) medicine.Description = request.Description;
        if (request.IsActive.HasValue) medicine.IsActive = request.IsActive.Value;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Medicine updated", null));
    }

    public async Task<IResult> DeleteMedicineAsync(ClaimsPrincipal user, Guid medicineId)
    {
        var clinicId = await GetClinicIdFromUser(user);
        if (clinicId == null)
            return Results.Unauthorized();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.MedicineId == medicineId && m.ClinicId == clinicId.Value);

        if (medicine == null)
            return Results.NotFound(new ApiResponse<object>(false, "Medicine not found", null));

        // Soft delete
        medicine.IsActive = false;
        medicine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Results.Ok(new ApiResponse<object>(true, "Medicine deleted", null));
    }
}
