using System.Text;
using ClinicManagement_API.Features.auth_service.endpoint;
using ClinicManagement_API.Features.auth_service.helper;
using ClinicManagement_API.Features.auth_service.service;
using ClinicManagement_API.Features.booking_service.service;
using ClinicManagement_API.Features.patient_service.endpoint;
using ClinicManagement_API.Features.patient_service.service;
using ClinicManagement_API.Infrastructure.Persisstence;
using ClinicManagement_API.Features.booking_service.endpoint;
using ClinicManagement_API.Features.receptionist_service.endpoint;
using ClinicManagement_API.Features.receptionist_service.service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);



var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING__CLINIC")
                    ?? builder.Configuration.GetConnectionString("Clinic_DB")
                    ?? throw new Exception("connectionString is missing");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new Exception("Missing valid jwt settings");
builder.Services.AddDbContext<ClinicDbContext>(option => option.UseNpgsql(connectionString));
builder.Services.AddIdentity<User, Role>(options =>
    {
        // Disable cookie-based authentication for API
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ClinicDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT as default authentication scheme
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
builder.Services.AddCors(option => option.AddPolicy("FE",
policy => policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));


// DI
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddTransient<JwtGenerator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinic Management API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



var app = builder.Build();
app.UseCors("FE");

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
app.MapTimeSlotsEndpoint();
app.MapReceptionistEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    await db.Database.MigrateAsync();
}
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
