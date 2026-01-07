using ClinicManagement_API.Domains.Enums;
using ClinicManagement_API.Features.auth_service.dto;
using ClinicManagement_API.Features.auth_service.helper;
using ClinicManagement_API.Infrastructure.Persisstence;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement_API.Features.auth_service.service;

public interface IAuthService
{
    Task<IResult> Register(RegisterDto dto);
    Task<IResult> Login(LoginDto dto);
}
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtGenerator _jwtGenerator;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        SignInManager<User> signInManager, 
        JwtGenerator jwtGenerator
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<IResult> Register(RegisterDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Username)) throw new Exception("Missing username");
            if (string.IsNullOrEmpty(dto.Password)) throw new Exception("Missing password");

            var existedUser = await _userManager.FindByNameAsync(dto.Username);
            if (existedUser != null) throw new Exception("Username is already used");
            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = dto.Username
            };
            var addUserStatus = await _userManager.CreateAsync(user, dto.Password);
            if (!addUserStatus.Succeeded)
                throw new Exception($"Cannot create new user {addUserStatus.Errors}");
            var existedRole = await _roleManager.RoleExistsAsync(AppRoles.User);
            if (!existedRole)
                await _roleManager.CreateAsync(new Role()
                {
                    Name = AppRoles.User
                });
            var isInRoles = await _userManager.IsInRoleAsync(user, AppRoles.User);
            if (!isInRoles) await _userManager.AddToRoleAsync(user, AppRoles.User);
            return Results.Ok(new { isSuccess = true, message = "Register successfully", userId = user.Id });
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    public async Task<IResult> Login(LoginDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Username)) 
                return Results.BadRequest("Username is missing");
            if (string.IsNullOrEmpty(dto.Password)) 
                return Results.BadRequest("Password is missing");

            var existedUser = await _userManager.FindByNameAsync(dto.Username);
            if (existedUser == null) return Results.BadRequest("User is not existed");
            var isPasswordMatched = await _signInManager.CheckPasswordSignInAsync(existedUser, dto.Password, false);
            if (!isPasswordMatched.Succeeded) return Results.BadRequest("Password is incorrect");

            var token = await _jwtGenerator.CreateTokenAsync(existedUser);
            return Results.Ok(new { existedUser.Id, accessToken = token });
        }
        catch (Exception e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}