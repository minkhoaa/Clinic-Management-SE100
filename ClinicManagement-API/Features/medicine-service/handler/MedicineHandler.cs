using System.Security.Claims;
using ClinicManagement_API.Features.medicine_service.dto;
using ClinicManagement_API.Features.medicine_service.service;

namespace ClinicManagement_API.Features.medicine_service.handler;

public static class MedicineHandler
{
    public static async Task<IResult> GetMedicines(IMedicineService service, ClaimsPrincipal user)
        => await service.GetMedicinesAsync(user);

    public static async Task<IResult> CreateMedicine(IMedicineService service, ClaimsPrincipal user,
        CreateMedicineRequest request)
        => await service.CreateMedicineAsync(user, request);

    public static async Task<IResult> UpdateMedicine(IMedicineService service, ClaimsPrincipal user, Guid medicineId,
        UpdateMedicineRequest request)
        => await service.UpdateMedicineAsync(user, medicineId, request);

    public static async Task<IResult> DeleteMedicine(IMedicineService service, ClaimsPrincipal user, Guid medicineId)
        => await service.DeleteMedicineAsync(user, medicineId);

    public static async Task<IResult> GetLowStock(IMedicineService service, ClaimsPrincipal user)
        => await service.GetLowStockAsync(user);

    public static async Task<IResult> UpdateStock(IMedicineService service, ClaimsPrincipal user, Guid medicineId,
        UpdateStockRequest request)
        => await service.UpdateStockAsync(user, medicineId, request);
}
