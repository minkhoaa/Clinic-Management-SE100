using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement_API.Infrastructure.Persisstence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Admin User
        await SeedAdminUserAsync(userManager);

        // Check if already seeded
        if (await context.Clinics.AnyAsync())
        {
            Console.WriteLine("Database already seeded.");
            return;
        }

        // Seed Clinics
        var clinics = await SeedClinicsAsync(context);

        // Seed Services
        var services = await SeedServicesAsync(context, clinics);

        // Seed Doctors
        var doctors = await SeedDoctorsAsync(context, clinics);

        // Seed Doctor-Service relationships
        await SeedDoctorServicesAsync(context, doctors, services);

        // Seed Doctor Availability
        await SeedDoctorAvailabilityAsync(context, clinics, doctors);

        // Seed Patients
        await SeedPatientsAsync(context, clinics);

        // Seed Staff Users
        await SeedStaffUsersAsync(context, clinics);

        Console.WriteLine("Database seeded successfully!");
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        var roles = new[] { CustomRoles.Admin, CustomRoles.Receptionist, CustomRoles.User };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
                Console.WriteLine($"Created role: {roleName}");
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager)
    {
        const string adminUsername = "admin";
        const string adminPassword = "Admin@123";

        var existingAdmin = await userManager.FindByNameAsync(adminUsername);
        if (existingAdmin == null)
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                UserName = adminUsername,
                Email = "admin@clinic.vn",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, CustomRoles.Admin);
                Console.WriteLine($"Created admin user: {adminUsername} / {adminPassword}");
            }
        }
    }

    private static async Task<List<Clinic>> SeedClinicsAsync(ClinicDbContext context)
    {
        var clinics = new List<Clinic>
        {
            new()
            {
                ClinicId = Guid.NewGuid(),
                Code = "PM-HCM-01",
                Name = "Phòng Mạch Hoàng Gia - 123 Nguyễn Huệ, Quận 1, HCM",
                TimeZone = "Asia/Ho_Chi_Minh",
                Phone = "028-1234-5678",
                Email = "hoanggia@clinic.vn",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                ClinicId = Guid.NewGuid(),
                Code = "PM-HCM-02",
                Name = "Phòng Khám Đa Khoa An Khang - 456 Lê Lợi, Quận 3, HCM",
                TimeZone = "Asia/Ho_Chi_Minh",
                Phone = "028-8765-4321",
                Email = "ankhang@clinic.vn",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Clinics.AddRange(clinics);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {clinics.Count} clinics");
        return clinics;
    }

    private static async Task<List<Service>> SeedServicesAsync(ClinicDbContext context, List<Clinic> clinics)
    {
        var services = new List<Service>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        // Services for Clinic 1
        services.AddRange(new[]
        {
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "KHAM-TQ",
                Name = "Khám Tổng Quát",
                DefaultDurationMin = 30,
                DefaultPrice = 300000,
                IsActive = true
            },
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "KHAM-NOI",
                Name = "Khám Nội Khoa",
                DefaultDurationMin = 30,
                DefaultPrice = 250000,
                IsActive = true
            },
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "KHAM-DA",
                Name = "Khám Da Liễu",
                DefaultDurationMin = 20,
                DefaultPrice = 200000,
                IsActive = true
            },
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "KHAM-TIM",
                Name = "Khám Tim Mạch",
                DefaultDurationMin = 45,
                DefaultPrice = 500000,
                IsActive = true
            }
        });

        // Services for Clinic 2
        services.AddRange(new[]
        {
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = "KHAM-TQ",
                Name = "Khám Tổng Quát",
                DefaultDurationMin = 30,
                DefaultPrice = 350000,
                IsActive = true
            },
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = "KHAM-RANG",
                Name = "Khám Răng Hàm Mặt",
                DefaultDurationMin = 30,
                DefaultPrice = 200000,
                IsActive = true
            },
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = "KHAM-MAT",
                Name = "Khám Mắt",
                DefaultDurationMin = 25,
                DefaultPrice = 180000,
                IsActive = true
            }
        });

        context.Services.AddRange(services);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {services.Count} services");
        return services;
    }

    private static async Task<List<Doctor>> SeedDoctorsAsync(ClinicDbContext context, List<Clinic> clinics)
    {
        var doctors = new List<Doctor>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        // Doctors for Clinic 1
        doctors.AddRange(new[]
        {
            new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "BS-001",
                FullName = "BS. Nguyễn Văn An",
                Specialty = "Nội Khoa",
                Phone = "0901234567",
                Email = "nguyen.an@clinic.vn",
                IsActive = true
            },
            new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "BS-002",
                FullName = "BS. Trần Thị Bình",
                Specialty = "Da Liễu",
                Phone = "0901234568",
                Email = "tran.binh@clinic.vn",
                IsActive = true
            },
            new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "BS-003",
                FullName = "TS.BS. Lê Hoàng Cường",
                Specialty = "Tim Mạch",
                Phone = "0901234569",
                Email = "le.cuong@clinic.vn",
                IsActive = true
            }
        });

        // Doctors for Clinic 2
        doctors.AddRange(new[]
        {
            new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = "BS-001",
                FullName = "BS. Phạm Minh Đức",
                Specialty = "Răng Hàm Mặt",
                Phone = "0902345678",
                Email = "pham.duc@clinic.vn",
                IsActive = true
            },
            new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = "BS-002",
                FullName = "BS. Võ Thị Hương",
                Specialty = "Nhãn Khoa",
                Phone = "0902345679",
                Email = "vo.huong@clinic.vn",
                IsActive = true
            }
        });

        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {doctors.Count} doctors");
        return doctors;
    }

    private static async Task SeedDoctorServicesAsync(ClinicDbContext context, List<Doctor> doctors, List<Service> services)
    {
        var doctorServices = new List<DoctorService>();

        // Link doctors to their relevant services
        foreach (var doctor in doctors)
        {
            var clinicServices = services.Where(s => s.ClinicId == doctor.ClinicId).ToList();
            
            // Each doctor provides general checkup + their specialty service
            var generalService = clinicServices.FirstOrDefault(s => s.Code == "KHAM-TQ");
            if (generalService != null)
            {
                doctorServices.Add(new DoctorService
                {
                    DoctorId = doctor.DoctorId,
                    ServiceId = generalService.ServiceId,
                    IsEnabled = true
                });
            }

            // Add specialty-specific services
            var specialtyService = doctor.Specialty switch
            {
                "Nội Khoa" => clinicServices.FirstOrDefault(s => s.Code == "KHAM-NOI"),
                "Da Liễu" => clinicServices.FirstOrDefault(s => s.Code == "KHAM-DA"),
                "Tim Mạch" => clinicServices.FirstOrDefault(s => s.Code == "KHAM-TIM"),
                "Răng Hàm Mặt" => clinicServices.FirstOrDefault(s => s.Code == "KHAM-RANG"),
                "Nhãn Khoa" => clinicServices.FirstOrDefault(s => s.Code == "KHAM-MAT"),
                _ => null
            };

            if (specialtyService != null)
            {
                doctorServices.Add(new DoctorService
                {
                    DoctorId = doctor.DoctorId,
                    ServiceId = specialtyService.ServiceId,
                    IsEnabled = true
                });
            }
        }

        context.DoctorServices.AddRange(doctorServices);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {doctorServices.Count} doctor-service links");
    }

    private static async Task SeedDoctorAvailabilityAsync(ClinicDbContext context, List<Clinic> clinics, List<Doctor> doctors)
    {
        var availabilities = new List<DoctorAvailability>();

        foreach (var doctor in doctors)
        {
            // Monday to Friday: 8:00 - 12:00 and 14:00 - 17:00
            for (byte dayOfWeek = 1; dayOfWeek <= 5; dayOfWeek++) // 1=Monday, 5=Friday
            {
                // Morning shift
                availabilities.Add(new DoctorAvailability
                {
                    AvailabilityId = Guid.NewGuid(),
                    ClinicId = doctor.ClinicId,
                    DoctorId = doctor.DoctorId,
                    DayOfWeek = dayOfWeek,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    SlotSizeMin = 30,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
                    EffectiveTo = DateTime.UtcNow.AddMonths(6)
                });

                // Afternoon shift
                availabilities.Add(new DoctorAvailability
                {
                    AvailabilityId = Guid.NewGuid(),
                    ClinicId = doctor.ClinicId,
                    DoctorId = doctor.DoctorId,
                    DayOfWeek = dayOfWeek,
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    SlotSizeMin = 30,
                    IsActive = true,
                    EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
                    EffectiveTo = DateTime.UtcNow.AddMonths(6)
                });
            }

            // Saturday: 8:00 - 12:00 only
            availabilities.Add(new DoctorAvailability
            {
                AvailabilityId = Guid.NewGuid(),
                ClinicId = doctor.ClinicId,
                DoctorId = doctor.DoctorId,
                DayOfWeek = 6, // Saturday
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                SlotSizeMin = 30,
                IsActive = true,
                EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
                EffectiveTo = DateTime.UtcNow.AddMonths(6)
            });
        }

        context.DoctorAvailabilities.AddRange(availabilities);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {availabilities.Count} doctor availability slots");
    }

    private static async Task SeedPatientsAsync(ClinicDbContext context, List<Clinic> clinics)
    {
        var patients = new List<Patients>();
        var clinic1 = clinics[0];

        patients.AddRange(new[]
        {
            new Patients
            {
                PatientId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                PatientCode = "BN-001",
                FullName = "Nguyễn Văn Khách",
                Gender = Gender.MALE,
                Dob = new DateTime(1990, 5, 15),
                PrimaryPhone = "0909123456",
                Email = "khach.nguyen@gmail.com",
                AddressLine1 = "789 Điện Biên Phủ, Quận Bình Thạnh"
            },
            new Patients
            {
                PatientId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                PatientCode = "BN-002",
                FullName = "Trần Thị Lan",
                Gender = Gender.FEMALE,
                Dob = new DateTime(1985, 8, 20),
                PrimaryPhone = "0909234567",
                Email = "lan.tran@gmail.com",
                AddressLine1 = "321 Cách Mạng Tháng 8, Quận 10"
            },
            new Patients
            {
                PatientId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                PatientCode = "BN-003",
                FullName = "Lê Minh Tuấn",
                Gender = Gender.MALE,
                Dob = new DateTime(1978, 12, 1),
                PrimaryPhone = "0909345678",
                Email = "tuan.le@gmail.com",
                AddressLine1 = "555 Võ Văn Tần, Quận 3"
            }
        });

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {patients.Count} patients");
    }

    private static async Task SeedStaffUsersAsync(ClinicDbContext context, List<Clinic> clinics)
    {
        var staffUsers = new List<StaffUser>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        staffUsers.AddRange(new[]
        {
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Username = "letan1",
                FullName = "Phạm Thị Lễ Tân",
                Role = StaffRole.Receptionist,
                IsActive = true
            },
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Username = "admin_hg",
                FullName = "Nguyễn Quản Lý",
                Role = StaffRole.Admin,
                IsActive = true
            },
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Username = "letan2",
                FullName = "Trần Văn Tiếp Tân",
                Role = StaffRole.Receptionist,
                IsActive = true
            }
        });

        context.StaffUsers.AddRange(staffUsers);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {staffUsers.Count} staff users");
    }
}
