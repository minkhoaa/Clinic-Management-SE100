using ClinicManagement_API.Features.booking_service.handler;

namespace ClinicManagement_API.Features.booking_service.endpoint;

public static class ServiceEndpoint
{
    public static void MapServiceEndpoint(this IEndpointRouteBuilder route)
    {
        var app = route.MapGroup("/api/admin/service").WithTags("Services Management");
        app.MapPost("/", AdminHandler.CreateService);
        app.MapPut("/{serviceId:guid}", AdminHandler.UpdateService);
        app.MapDelete("/{serviceId:guid}", AdminHandler.DeleteService);
        app.MapGet("/", AdminHandler.GetAllServices);
        
    }
}