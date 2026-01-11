using System.Text;
using ClinicManagement_API.Features.auth_service.endpoint;
using ClinicManagement_API.Features.auth_service.helper;
using ClinicManagement_API.Features.auth_service.service;
using ClinicManagement_API.Features.billing_service.endpoint;
using ClinicManagement_API.Features.billing_service.helper;
using ClinicManagement_API.Features.booking_service.service;
using ClinicManagement_API.Features.patient_service.endpoint;
using ClinicManagement_API.Features.patient_service.service;
using ClinicManagement_API.Infrastructure.Persisstence;
using ClinicManagement_API.Features.booking_service.endpoint;
using ClinicManagement_API.Features.receptionist_service.endpoint;
using ClinicManagement_API.Features.receptionist_service.service;
using ClinicManagement_API.Features.billing_service.service;
using ClinicManagement_API.Features.doctor_service.endpoint;
using ClinicManagement_API.Features.doctor_service.service;
using ClinicManagement_API.Features.medicine_service.service;
using ClinicManagement_API.Features.medicine_service.endpoint;
using ClinicManagement_API.Features.admin_service.service;
using ClinicManagement_API.Features.admin_service.endpoint;
using ClinicManagement_API.Features.email_service;
using ClinicManagement_API.Domains.Enums;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Load .env file if exists (for local development)
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Connection string from ENV or appsettings
var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__CLINIC_DB")
                       ?? builder.Configuration.GetConnectionString("Clinic_DB")
                       ?? throw new Exception("connectionString is missing");

// JWT Settings from ENV
var jwtKey = Environment.GetEnvironmentVariable("JWTSETTINGS__KEY")
             ?? builder.Configuration["JwtSettings:Key"] ?? throw new Exception("JWT Key missing");
var jwtIssuer = Environment.GetEnvironmentVariable("JWTSETTINGS__ISSUER")
                ?? builder.Configuration["JwtSettings:Issuer"] ?? "ClinicApi";
var jwtAudience = Environment.GetEnvironmentVariable("JWTSETTINGS__AUDIENCE")
                  ?? builder.Configuration["JwtSettings:Audience"] ?? "ClinicApiClient";

// Register JwtSettings with IOptions pattern
builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtKey;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
});

// VnPay Options from ENV
builder.Services.Configure<VnPayOptions>(options =>
{
    options.TmnCode = Environment.GetEnvironmentVariable("VNPAY__TMNCODE")
                      ?? builder.Configuration["VnPay:TmnCode"] ?? "";
    options.HashSecret = Environment.GetEnvironmentVariable("VNPAY__HASHSECRET")
                         ?? builder.Configuration["VnPay:HashSecret"] ?? "";
    options.PaymentUrl = Environment.GetEnvironmentVariable("VNPAY__PAYMENTURL")
                         ?? builder.Configuration["VnPay:PaymentUrl"] ?? "";
    options.ReturnUrl = Environment.GetEnvironmentVariable("VNPAY__RETURNURL")
                        ?? builder.Configuration["VnPay:ReturnUrl"] ?? "";
    options.IpnUrl = Environment.GetEnvironmentVariable("VNPAY__IPNURL")
                     ?? builder.Configuration["VnPay:IpnUrl"] ?? "";
    options.Locale = Environment.GetEnvironmentVariable("VNPAY__LOCALE")
                     ?? builder.Configuration["VnPay:Locale"] ?? "vn";
});

builder.Services.AddDbContext<ClinicDbContext>(option => option.UseNpgsql(connectionString));
builder.Services.AddIdentity<User, Role>(options =>
    {
        // Disable cookie-based authentication for API
        options.SignIn.RequireConfirmedAccount = false;

        // Relax password requirements for auto-generated passwords (phone numbers)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
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
            ValidAudience = jwtAudience,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole(AppRoles.Doctor));
    options.AddPolicy("ReceptionistOnly", policy => policy.RequireRole(AppRoles.Receptionist));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole(AppRoles.Patient));
    options.AddPolicy("StaffOnly",
        policy => policy.RequireRole(AppRoles.Admin, AppRoles.Doctor, AppRoles.Receptionist));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole(AppRoles.Admin, AppRoles.Doctor));
    options.AddPolicy("ReceptionistOrAdmin", policy => policy.RequireRole(AppRoles.Admin, AppRoles.Receptionist));
});
builder.Services.AddCors(option => option.AddPolicy("FE",
    policy => policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// FluentEmail configuration (from ENV or appsettings)
// Helper to get non-empty env var
string? GetEnvOrNull(string key) =>
    Environment.GetEnvironmentVariable(key) is { Length: > 0 } val ? val : null;

var smtpHost = GetEnvOrNull("EMAIL__SMTPHOST")
               ?? builder.Configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
var smtpPort = int.Parse(GetEnvOrNull("EMAIL__SMTPPORT")
                         ?? builder.Configuration["Email:SmtpPort"] ?? "587");
var smtpUser = GetEnvOrNull("EMAIL__USERNAME")
               ?? builder.Configuration["Email:Username"] ?? "";
var smtpPass = GetEnvOrNull("EMAIL__PASSWORD")
               ?? builder.Configuration["Email:Password"] ?? "";
var fromEmail = GetEnvOrNull("EMAIL__FROMEMAIL")
                ?? builder.Configuration["Email:FromEmail"] ?? "noreply@clinic.com";
var fromName = GetEnvOrNull("EMAIL__FROMNAME")
               ?? builder.Configuration["Email:FromName"] ?? "Clinic";

builder.Services
    .AddFluentEmail(fromEmail, fromName)
    .AddSmtpSender(smtpHost, smtpPort, smtpUser, smtpPass);

// DI
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IReceptionistService, ReceptionistService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IDoctorPracticeService, DoctorPracticeService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IAdminReportService, AdminReportService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IBookingEmailService, BookingEmailService>();
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
app.MapDoctorPracticeEndpoint();
app.MapMedicineEndpoints();
app.MapBillingEndpoint();
app.MapAdminReportEndpoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
