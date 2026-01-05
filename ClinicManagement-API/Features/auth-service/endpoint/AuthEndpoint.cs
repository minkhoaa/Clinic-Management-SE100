using ClinicManagement_API.Features.auth_service.handler;

namespace ClinicManagement_API.Features.auth_service.endpoint;

public static class AuthEndpoint
{
    public static void MapAuthEndpoint(this IEndpointRouteBuilder router)
    {
        var app = router.MapGroup("/api/auth");
        app.MapPost("/register", AuthHandler.RegisterHandler);
        app.MapPost("/login", AuthHandler.LoginHandler);
    }
}