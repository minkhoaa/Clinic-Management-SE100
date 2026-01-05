using System.Text;
using ClinicManagement_API.Features.auth_service.endpoint;
using ClinicManagement_API.Features.auth_service.helper;
using ClinicManagement_API.Features.auth_service.service;
using ClinicManagement_API.Features.booking_service.service;
using ClinicManagement_API.Infrastructure.Persisstence;
using ClinicManagement_API.Features.booking_service.endpoint;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);



var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING__CLINIC")
                    ?? builder.Configuration.GetConnectionString("Clinic_DB")
                    ?? throw new Exception("connectionString is missing");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
    ?? throw new Exception("Missing valid jwt settings");
builder.Services.AddDbContext<ClinicDbContext>(option => option.UseNpgsql(connectionString));
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ClinicDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidAudience = jwtSettings.Audience,
            ValidIssuer = jwtSettings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)), 
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();



// DI
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<JwtGenerator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
app.MapBookingClinicEndpoint();
app.MapBookingServiceEndpoint();
app.MapBookingDoctorEndpoint();
app.MapBookingSlotEndpoint();
app.MapBookingEndpoint();
app.MapEnumEndpoint();
app.MapClinicEndpoint();
app.MapDoctorEndpoint();
app.MapStaffUserEndpoint();
app.MapPatientEndpoint();
app.MapServiceEndpoint();
app.MapAuthEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    await db.Database.MigrateAsync();
    
    // Seed sample data
    await DbSeeder.SeedAsync(app.Services);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
