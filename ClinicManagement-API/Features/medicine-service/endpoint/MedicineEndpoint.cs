using ClinicManagement_API.Features.medicine_service.handler;

namespace ClinicManagement_API.Features.medicine_service.endpoint;

public static class MedicineEndpoint
{
    public static void MapMedicineEndpoints(this IEndpointRouteBuilder app)
    {
        var medicines = app.MapGroup("/api/medicines")
            .WithTags("Medicines Management")
            .RequireAuthorization("StaffOnly");

        medicines.MapGet("", MedicineHandler.GetMedicines);
        medicines.MapPost("", MedicineHandler.CreateMedicine);
        medicines.MapPut("{medicineId:guid}", MedicineHandler.UpdateMedicine);
        medicines.MapDelete("{medicineId:guid}", MedicineHandler.DeleteMedicine);
    }
}
