using ClinicManagement_API.Features.auth_service.dto;
using ClinicManagement_API.Features.auth_service.service;

namespace ClinicManagement_API.Features.auth_service.handler;

public static class AuthHandler
{
    public static Task<IResult> RegisterHandler(RegisterDto dto, IAuthService service)
        => service.Register(dto);
    public static Task<IResult> LoginHandler(LoginDto dto, IAuthService service)
        => service.Login(dto);
}