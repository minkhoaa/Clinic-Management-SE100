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

        // Seed Admin Users
        await SeedAdminUsersAsync(userManager);

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

        // Seed Doctors with User accounts
        var doctors = await SeedDoctorsAsync(context, userManager, clinics);

        // Seed Doctor-Service relationships
        await SeedDoctorServicesAsync(context, doctors, services);

        // Seed Doctor Availability
        await SeedDoctorAvailabilityAsync(context, doctors);

        // Seed Medicines
        var medicines = await SeedMedicinesAsync(context, clinics);

        // Seed Patients with User accounts
        var patients = await SeedPatientsAsync(context, userManager, clinics);

        // Seed Staff Users (Receptionists)
        await SeedStaffUsersAsync(context, userManager, clinics);

        // Seed Appointments with various statuses
        var appointments = await SeedAppointmentsAsync(context, clinics, doctors, services, patients);

        // Seed Medical Records for completed appointments
        await SeedMedicalRecordsAsync(context, appointments, doctors, patients, clinics);

        // Seed Bills and BillItems
        await SeedBillsAsync(context, appointments, services, medicines, patients, clinics);

        // Seed Prescription Templates
        await SeedPrescriptionTemplatesAsync(context, clinics, doctors, medicines);

        Console.WriteLine("========================================");
        Console.WriteLine("Database seeded successfully!");
        Console.WriteLine("========================================");
        PrintTestAccounts();
    }

    private static void PrintTestAccounts()
    {
        Console.WriteLine("\n=== TEST ACCOUNTS ===");
        Console.WriteLine("Admin:");
        Console.WriteLine("  - admin / Admin@123");
        Console.WriteLine("  - superadmin / Admin@123");
        Console.WriteLine("\nReceptionist:");
        Console.WriteLine("  - letan1 / Letan@123");
        Console.WriteLine("  - letan2 / Letan@123");
        Console.WriteLine("\nDoctor:");
        Console.WriteLine("  - doctor.an / Doctor@123");
        Console.WriteLine("  - doctor.binh / Doctor@123");
        Console.WriteLine("  - doctor.cuong / Doctor@123");
        Console.WriteLine("  - doctor.duc / Doctor@123");
        Console.WriteLine("  - doctor.huong / Doctor@123");
        Console.WriteLine("\nPatient:");
        Console.WriteLine("  - patient.khach / Patient@123");
        Console.WriteLine("  - patient.lan / Patient@123");
        Console.WriteLine("  - patient.tuan / Patient@123");
        Console.WriteLine("  - patient.mai / Patient@123");
        Console.WriteLine("  - patient.hai / Patient@123");
        Console.WriteLine("======================\n");
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        var roles = new[] { AppRoles.Admin, AppRoles.Receptionist, AppRoles.Doctor, AppRoles.Patient };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
                Console.WriteLine($"Created role: {roleName}");
            }
        }
    }

    private static async Task SeedAdminUsersAsync(UserManager<User> userManager)
    {
        // Admin 1 - Primary Admin
        await CreateUserIfNotExists(userManager, new User
        {
            UserName = "admin",
            Email = "admin@clinic.vn",
            EmailConfirmed = true
        }, "Admin@123", AppRoles.Admin);

        // Admin 2 - Super Admin (for edge case testing)
        await CreateUserIfNotExists(userManager, new User
        {
            UserName = "superadmin",
            Email = "superadmin@clinic.vn",
            EmailConfirmed = true
        }, "Admin@123", AppRoles.Admin);
    }

    private static async Task<IdentityResult?> CreateUserIfNotExists(
        UserManager<User> userManager, User user, string password, string role)
    {
        var existingUser = await userManager.FindByNameAsync(user.UserName!);
        if (existingUser != null)
        {
            Console.WriteLine($"User {user.UserName} already exists, skipping...");
            return null;
        }

        user.Id = Guid.NewGuid();
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"Created {role} user: {user.UserName} / {password}");
        }
        else
        {
            Console.WriteLine(
                $"FAILED to create user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return result;
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
            },
            // Inactive service for edge case testing
            new Service
            {
                ServiceId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = "KHAM-PHOI",
                Name = "Khám Phổi (Ngưng hoạt động)",
                DefaultDurationMin = 30,
                DefaultPrice = 280000,
                IsActive = false
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
        Console.WriteLine($"Created {services.Count} services (including 1 inactive)");
        return services;
    }

    private static async Task<List<Doctor>> SeedDoctorsAsync(
        ClinicDbContext context, UserManager<User> userManager, List<Clinic> clinics)
    {
        var doctors = new List<Doctor>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        // Doctors for Clinic 1
        var doctorData1 = new[]
        {
            ("BS-001", "BS. Nguyễn Văn An", "Nội Khoa", "0901234567", "nguyen.an@clinic.vn", "doctor.an"),
            ("BS-002", "BS. Trần Thị Bình", "Da Liễu", "0901234568", "tran.binh@clinic.vn", "doctor.binh"),
            ("BS-003", "TS.BS. Lê Hoàng Cường", "Tim Mạch", "0901234569", "le.cuong@clinic.vn", "doctor.cuong")
        };

        foreach (var (code, fullName, specialty, phone, email, username) in doctorData1)
        {
            // Create User account for doctor
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };
            await CreateUserIfNotExists(userManager, user, "Doctor@123", AppRoles.Doctor);

            doctors.Add(new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = code,
                FullName = fullName,
                Specialty = specialty,
                Phone = phone,
                Email = email,
                IsActive = true
            });
        }

        // Doctors for Clinic 2
        var doctorData2 = new[]
        {
            ("BS-001", "BS. Phạm Minh Đức", "Răng Hàm Mặt", "0902345678", "pham.duc@clinic.vn", "doctor.duc"),
            ("BS-002", "BS. Võ Thị Hương", "Nhãn Khoa", "0902345679", "vo.huong@clinic.vn", "doctor.huong")
        };

        foreach (var (code, fullName, specialty, phone, email, username) in doctorData2)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };
            await CreateUserIfNotExists(userManager, user, "Doctor@123", AppRoles.Doctor);

            doctors.Add(new Doctor
            {
                DoctorId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = code,
                FullName = fullName,
                Specialty = specialty,
                Phone = phone,
                Email = email,
                IsActive = true
            });
        }

        // Add one inactive doctor for edge case testing
        doctors.Add(new Doctor
        {
            DoctorId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            Code = "BS-004",
            FullName = "BS. Trương Văn Nhân (Nghỉ việc)",
            Specialty = "Nội Khoa",
            Phone = "0901234570",
            Email = "truong.nhan@clinic.vn",
            IsActive = false
        });

        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {doctors.Count} doctors (including 1 inactive)");
        return doctors;
    }

    private static async Task SeedDoctorServicesAsync(ClinicDbContext context, List<Doctor> doctors,
        List<Service> services)
    {
        var doctorServices = new List<DoctorService>();

        foreach (var doctor in doctors.Where(d => d.IsActive))
        {
            var clinicServices = services.Where(s => s.ClinicId == doctor.ClinicId && s.IsActive).ToList();

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

    private static async Task SeedDoctorAvailabilityAsync(ClinicDbContext context, List<Doctor> doctors)
    {
        var availabilities = new List<DoctorAvailability>();

        foreach (var doctor in doctors.Where(d => d.IsActive))
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

    private static async Task<List<Medicine>> SeedMedicinesAsync(ClinicDbContext context, List<Clinic> clinics)
    {
        var medicines = new List<Medicine>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        var medicineData = new[]
        {
            ("PARA-500", "Paracetamol 500mg", "Viên", 2000m, "Thuốc giảm đau, hạ sốt"),
            ("AMOX-500", "Amoxicillin 500mg", "Viên", 3000m, "Kháng sinh phổ rộng"),
            ("IBUP-400", "Ibuprofen 400mg", "Viên", 2500m, "Thuốc giảm đau, kháng viêm"),
            ("OMEP-20", "Omeprazole 20mg", "Viên", 4000m, "Thuốc dạ dày"),
            ("CEFI-200", "Cefixime 200mg", "Viên", 8000m, "Kháng sinh cephalosporin"),
            ("LORA-10", "Loratadine 10mg", "Viên", 3500m, "Thuốc chống dị ứng"),
            ("DEXT-15", "Dextromethorphan 15mg", "Viên", 2500m, "Thuốc ho"),
            ("VITA-C", "Vitamin C 500mg", "Viên", 1500m, "Bổ sung vitamin C"),
            ("PRED-5", "Prednisolone 5mg", "Viên", 1000m, "Thuốc kháng viêm corticoid"),
            ("MONT-10", "Montelukast 10mg", "Viên", 5000m, "Thuốc hen suyễn"),
            ("METO-500", "Metformin 500mg", "Viên", 1200m, "Thuốc tiểu đường"),
            ("AMLO-5", "Amlodipine 5mg", "Viên", 2000m, "Thuốc huyết áp"),
            ("NAC-200", "NAC 200mg", "Gói", 3000m, "Thuốc long đờm, giảm ho"),
            ("STREP-SPRAY", "Strepsils Spray", "Chai", 85000m, "Xịt họng giảm đau"),
            ("EYE-DROP", "Nước mắt nhân tạo", "Lọ", 45000m, "Dưỡng mắt, giảm khô mắt")
        };

        // Medicines for Clinic 1
        foreach (var (code, name, unit, price, desc) in medicineData)
        {
            medicines.Add(new Medicine
            {
                MedicineId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Code = code,
                Name = name,
                Unit = unit,
                Price = price,
                Description = desc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Medicines for Clinic 2 (subset)
        foreach (var (code, name, unit, price, desc) in medicineData.Take(8))
        {
            medicines.Add(new Medicine
            {
                MedicineId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Code = code,
                Name = name,
                Unit = unit,
                Price = price,
                Description = desc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Add one inactive medicine for edge case testing
        medicines.Add(new Medicine
        {
            MedicineId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            Code = "OLD-MED",
            Name = "Thuốc cũ (Ngưng sử dụng)",
            Unit = "Viên",
            Price = 1000m,
            Description = "Thuốc không còn sử dụng",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        context.Medicines.AddRange(medicines);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {medicines.Count} medicines (including 1 inactive)");
        return medicines;
    }

    private static async Task<List<Patients>> SeedPatientsAsync(
        ClinicDbContext context, UserManager<User> userManager, List<Clinic> clinics)
    {
        var patients = new List<Patients>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        var patientData = new[]
        {
            (clinic1.ClinicId, "BN-001", "Nguyễn Văn Khách", Gender.MALE,
                new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                "0909123456", "khach.nguyen@gmail.com", "789 Điện Biên Phủ, Quận Bình Thạnh", "patient.khach",
                null as string, null as string, "Nguyễn Thị Mẹ", "0909111222", "O+", null as string, null as string),

            (clinic1.ClinicId, "BN-002", "Trần Thị Lan", Gender.FEMALE,
                new DateTime(1985, 8, 20, 0, 0, 0, DateTimeKind.Utc),
                "0909234567", "lan.tran@gmail.com", "321 Cách Mạng Tháng 8, Quận 10", "patient.lan",
                "Penicillin", "Tiểu đường type 2", "Trần Văn Chồng", "0909222333", "A+", "BHYT", "DN123456789"),

            (clinic1.ClinicId, "BN-003", "Lê Minh Tuấn", Gender.MALE,
                new DateTime(1978, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                "0909345678", "tuan.le@gmail.com", "555 Võ Văn Tần, Quận 3", "patient.tuan",
                null as string, "Cao huyết áp", "Lê Thị Vợ", "0909333444", "B+", null as string, null as string),

            (clinic1.ClinicId, "BN-004", "Phạm Thị Mai", Gender.FEMALE,
                new DateTime(1995, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                "0909456789", "mai.pham@gmail.com", "123 Nguyễn Thị Minh Khai, Quận 1", "patient.mai",
                "Aspirin", null as string, "Phạm Văn Cha", "0909444555", "AB+", "BHYT", "DN987654321"),

            (clinic2.ClinicId, "BN-001", "Võ Văn Hải", Gender.MALE,
                new DateTime(2000, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                "0909567890", "hai.vo@gmail.com", "456 Lý Thường Kiệt, Quận Tân Bình", "patient.hai",
                null as string, null as string, null as string, null as string, null as string, null as string,
                null as string)
        };

        foreach (var (clinicId, code, fullName, gender, dob, phone, email, address, username,
                     allergy, chronic, emergencyName, emergencyPhone, bloodGroup, insuranceType, insuranceNumber) in
                 patientData)
        {
            // Create User account for patient
            var user = new User
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };
            var result = await CreateUserIfNotExists(userManager, user, "Patient@123", AppRoles.Patient);
            var userId = await userManager.FindByNameAsync(username);

            patients.Add(new Patients
            {
                PatientId = Guid.NewGuid(),
                ClinicId = clinicId,
                UserId = userId?.Id,
                PatientCode = code,
                FullName = fullName,
                Gender = gender,
                Dob = dob,
                PrimaryPhone = phone,
                Email = email,
                AddressLine1 = address,
                Allergy = allergy,
                ChronicDisease = chronic,
                EmergencyName = emergencyName,
                EmergencyPhone = emergencyPhone,
                BloodGroup = bloodGroup,
                InsuranceType = insuranceType,
                InsuranceNumber = insuranceNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        context.Patients.AddRange(patients);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {patients.Count} patients with user accounts");
        return patients;
    }

    private static async Task SeedStaffUsersAsync(
        ClinicDbContext context, UserManager<User> userManager, List<Clinic> clinics)
    {
        var staffUsers = new List<StaffUser>();
        var clinic1 = clinics[0];
        var clinic2 = clinics[1];

        // Create receptionist user accounts
        await CreateUserIfNotExists(userManager, new User
        {
            UserName = "letan1",
            Email = "letan1@clinic.vn",
            EmailConfirmed = true
        }, "Letan@123", AppRoles.Receptionist);

        await CreateUserIfNotExists(userManager, new User
        {
            UserName = "letan2",
            Email = "letan2@clinic.vn",
            EmailConfirmed = true
        }, "Letan@123", AppRoles.Receptionist);

        staffUsers.AddRange(new[]
        {
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Username = "letan1",
                FullName = "Phạm Thị Lễ Tân",
                Role = AppRoles.Receptionist,
                IsActive = true
            },
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Username = "admin_hg",
                FullName = "Nguyễn Quản Lý",
                Role = AppRoles.Admin,
                IsActive = true
            },
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic2.ClinicId,
                Username = "letan2",
                FullName = "Trần Văn Tiếp Tân",
                Role = AppRoles.Receptionist,
                IsActive = true
            },
            // Inactive staff for edge case testing
            new StaffUser
            {
                UserId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                Username = "letan_old",
                FullName = "Nguyễn Văn Cũ (Nghỉ việc)",
                Role = AppRoles.Receptionist,
                IsActive = false
            }
        });

        context.StaffUsers.AddRange(staffUsers);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {staffUsers.Count} staff users (including 1 inactive)");
    }

    private static async Task<List<Appointment>> SeedAppointmentsAsync(
        ClinicDbContext context, List<Clinic> clinics, List<Doctor> doctors,
        List<Service> services, List<Patients> patients)
    {
        var appointments = new List<Appointment>();
        var clinic1 = clinics[0];
        var clinic1Doctors = doctors.Where(d => d.ClinicId == clinic1.ClinicId && d.IsActive).ToList();
        var clinic1Services = services.Where(s => s.ClinicId == clinic1.ClinicId && s.IsActive).ToList();
        var clinic1Patients = patients.Where(p => p.ClinicId == clinic1.ClinicId).ToList();

        var now = DateTime.UtcNow;

        // === COMPLETED appointments (past) ===
        for (int i = 0; i < 5; i++)
        {
            var doctor = clinic1Doctors[i % clinic1Doctors.Count];
            var service = clinic1Services[i % clinic1Services.Count];
            var patient = clinic1Patients[i % clinic1Patients.Count];
            var startAt = now.AddDays(-10 + i).Date.AddHours(9 + i);

            appointments.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                DoctorId = doctor.DoctorId,
                ServiceId = service.ServiceId,
                PatientId = patient.PatientId,
                StartAt = startAt,
                EndAt = startAt.AddMinutes(service.DefaultDurationMin ?? 30),
                Source = AppointmentSource.Web,
                ContactFullName = patient.FullName,
                ContactPhone = patient.PrimaryPhone!,
                ContactEmail = patient.Email,
                Notes = $"Lịch hẹn đã hoàn thành #{i + 1}",
                Status = AppointmentStatus.Completed,
                ActualStartTime = startAt.AddMinutes(5),
                ActualEndTime = startAt.AddMinutes(35),
                CreatedAt = startAt.AddDays(-3),
                UpdatedAt = startAt.AddMinutes(35)
            });
        }

        // === CONFIRMED appointments (future) ===
        for (int i = 0; i < 3; i++)
        {
            var doctor = clinic1Doctors[i % clinic1Doctors.Count];
            var service = clinic1Services[i % clinic1Services.Count];
            var patient = clinic1Patients[i % clinic1Patients.Count];
            var startAt = now.AddDays(1 + i).Date.AddHours(10 + i);

            appointments.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                DoctorId = doctor.DoctorId,
                ServiceId = service.ServiceId,
                PatientId = patient.PatientId,
                StartAt = startAt,
                EndAt = startAt.AddMinutes(service.DefaultDurationMin ?? 30),
                Source = AppointmentSource.App,
                ContactFullName = patient.FullName,
                ContactPhone = patient.PrimaryPhone!,
                ContactEmail = patient.Email,
                Notes = $"Lịch hẹn đã xác nhận #{i + 1}",
                Status = AppointmentStatus.Confirmed,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now
            });
        }

        // === PENDING appointments (future) ===
        for (int i = 0; i < 3; i++)
        {
            var doctor = clinic1Doctors[(i + 1) % clinic1Doctors.Count];
            var service = clinic1Services[(i + 1) % clinic1Services.Count];
            var patient = clinic1Patients[(i + 1) % clinic1Patients.Count];
            var startAt = now.AddDays(3 + i).Date.AddHours(14 + i);

            appointments.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                DoctorId = doctor.DoctorId,
                ServiceId = service.ServiceId,
                PatientId = patient.PatientId,
                StartAt = startAt,
                EndAt = startAt.AddMinutes(service.DefaultDurationMin ?? 30),
                Source = AppointmentSource.Hotline,
                ContactFullName = patient.FullName,
                ContactPhone = patient.PrimaryPhone!,
                ContactEmail = patient.Email,
                Notes = $"Lịch hẹn chờ xác nhận #{i + 1}",
                Status = AppointmentStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        // === CHECKED-IN appointment (today) ===
        var checkinDoctor = clinic1Doctors[0];
        var checkinService = clinic1Services[0];
        var checkinPatient = clinic1Patients[0];
        var checkinTime = now.Date.AddHours(now.Hour + 1);

        appointments.Add(new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            DoctorId = checkinDoctor.DoctorId,
            ServiceId = checkinService.ServiceId,
            PatientId = checkinPatient.PatientId,
            StartAt = checkinTime,
            EndAt = checkinTime.AddMinutes(30),
            Source = AppointmentSource.FrontDesk,
            ContactFullName = checkinPatient.FullName,
            ContactPhone = checkinPatient.PrimaryPhone!,
            ContactEmail = checkinPatient.Email,
            Notes = "Bệnh nhân đã check-in, đang chờ khám",
            Status = AppointmentStatus.CheckedIn,
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now
        });

        // === IN-PROGRESS appointment (today) ===
        var inProgressDoctor = clinic1Doctors[1];
        var inProgressService = clinic1Services[1];
        var inProgressPatient = clinic1Patients[1];
        var inProgressTime = now.Date.AddHours(now.Hour);

        appointments.Add(new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            DoctorId = inProgressDoctor.DoctorId,
            ServiceId = inProgressService.ServiceId,
            PatientId = inProgressPatient.PatientId,
            StartAt = inProgressTime,
            EndAt = inProgressTime.AddMinutes(30),
            Source = AppointmentSource.Web,
            ContactFullName = inProgressPatient.FullName,
            ContactPhone = inProgressPatient.PrimaryPhone!,
            ContactEmail = inProgressPatient.Email,
            Notes = "Bác sĩ đang khám",
            Status = AppointmentStatus.InProgress,
            ActualStartTime = inProgressTime.AddMinutes(5),
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now
        });

        // === CANCELLED appointment ===
        var cancelDoctor = clinic1Doctors[0];
        var cancelService = clinic1Services[0];
        var cancelPatient = clinic1Patients[2];
        var cancelTime = now.AddDays(-2).Date.AddHours(11);

        appointments.Add(new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            DoctorId = cancelDoctor.DoctorId,
            ServiceId = cancelService.ServiceId,
            PatientId = cancelPatient.PatientId,
            StartAt = cancelTime,
            EndAt = cancelTime.AddMinutes(30),
            Source = AppointmentSource.Web,
            ContactFullName = cancelPatient.FullName,
            ContactPhone = cancelPatient.PrimaryPhone!,
            ContactEmail = cancelPatient.Email,
            Notes = "Bệnh nhân hủy vì bận việc",
            Status = AppointmentStatus.Cancelled,
            CreatedAt = cancelTime.AddDays(-5),
            UpdatedAt = cancelTime.AddDays(-1)
        });

        // === NO-SHOW appointment ===
        var noShowDoctor = clinic1Doctors[1];
        var noShowService = clinic1Services[1];
        var noShowPatient = clinic1Patients[3];
        var noShowTime = now.AddDays(-5).Date.AddHours(10);

        appointments.Add(new Appointment
        {
            AppointmentId = Guid.NewGuid(),
            ClinicId = clinic1.ClinicId,
            DoctorId = noShowDoctor.DoctorId,
            ServiceId = noShowService.ServiceId,
            PatientId = noShowPatient.PatientId,
            StartAt = noShowTime,
            EndAt = noShowTime.AddMinutes(30),
            Source = AppointmentSource.App,
            ContactFullName = noShowPatient.FullName,
            ContactPhone = noShowPatient.PrimaryPhone!,
            ContactEmail = noShowPatient.Email,
            Notes = "Bệnh nhân không đến",
            Status = AppointmentStatus.NoShow,
            CreatedAt = noShowTime.AddDays(-3),
            UpdatedAt = noShowTime.AddHours(1)
        });

        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
        Console.WriteLine(
            $"Created {appointments.Count} appointments (Completed: 5, Confirmed: 3, Pending: 3, CheckedIn: 1, InProgress: 1, Cancelled: 1, NoShow: 1)");
        return appointments;
    }

    private static async Task SeedMedicalRecordsAsync(
        ClinicDbContext context, List<Appointment> appointments,
        List<Doctor> doctors, List<Patients> patients, List<Clinic> clinics)
    {
        var completedAppointments = appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        var medicalRecords = new List<MedicalRecord>();

        var diagnosisData = new[]
        {
            ("Khám định kỳ", "Sức khỏe tổng quát tốt", "Theo dõi, tái khám sau 6 tháng", "Vitamin C 500mg x 30 viên"),
            ("Cảm cúm thông thường", "Cảm cúm do virus", "Nghỉ ngơi, uống nhiều nước",
                "Paracetamol 500mg x 10 viên, Vitamin C x 15 viên"),
            ("Viêm họng", "Viêm họng cấp", "Kháng sinh, thuốc ho",
                "Amoxicillin 500mg x 21 viên, Dextromethorphan x 15 viên"),
            ("Đau đầu", "Đau đầu căng thẳng", "Nghỉ ngơi, giảm stress", "Paracetamol 500mg x 10 viên"),
            ("Khám da liễu", "Dị ứng da nhẹ", "Thuốc chống dị ứng",
                "Loratadine 10mg x 10 viên, Prednisolone 5mg x 7 viên")
        };

        for (int i = 0; i < completedAppointments.Count && i < diagnosisData.Length; i++)
        {
            var appointment = completedAppointments[i];
            var (title, diagnosis, treatment, prescription) = diagnosisData[i];

            medicalRecords.Add(new MedicalRecord
            {
                RecordId = Guid.NewGuid(),
                PatientId = appointment.PatientId!.Value,
                ClinicId = appointment.ClinicId,
                DoctorId = appointment.DoctorId,
                AppointmentId = appointment.AppointmentId,
                Title = title,
                RecordDate = appointment.StartAt,
                Diagnosis = diagnosis,
                Treatment = treatment,
                Prescription = prescription,
                Notes = $"Bệnh nhân hợp tác tốt. Hẹn tái khám nếu có triệu chứng bất thường.",
                CreatedAt = appointment.ActualEndTime ?? appointment.EndAt,
                UpdatedAt = appointment.ActualEndTime ?? appointment.EndAt
            });
        }

        context.MedicalRecords.AddRange(medicalRecords);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {medicalRecords.Count} medical records");
    }

    private static async Task SeedBillsAsync(
        ClinicDbContext context, List<Appointment> appointments,
        List<Service> services, List<Medicine> medicines,
        List<Patients> patients, List<Clinic> clinics)
    {
        var bills = new List<Bill>();
        var billItems = new List<BillItem>();
        var clinic1 = clinics[0];
        var clinic1Medicines = medicines.Where(m => m.ClinicId == clinic1.ClinicId && m.IsActive).ToList();

        var completedAppointments = appointments.Where(a => a.Status == AppointmentStatus.Completed).ToList();
        var invoiceCounter = 1;

        // PAID bills for completed appointments
        foreach (var appointment in completedAppointments)
        {
            var service = services.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
            if (service == null) continue;

            var bill = new Bill
            {
                BillId = Guid.NewGuid(),
                ClinicId = appointment.ClinicId,
                PatientId = appointment.PatientId!.Value,
                AppointmentId = appointment.AppointmentId,
                InvoiceNumber = $"HD-2026-{invoiceCounter++:D4}",
                Status = BillStatus.Paid,
                Subtotal = 0,
                Discount = 0,
                TotalAmount = 0,
                PaymentMethod = invoiceCounter % 3 == 0 ? PaymentMethod.Transfer :
                    invoiceCounter % 2 == 0 ? PaymentMethod.Card : PaymentMethod.Cash,
                CreatedAt = appointment.ActualEndTime ?? appointment.EndAt,
                PaymentDate = appointment.ActualEndTime?.AddMinutes(10) ?? appointment.EndAt.AddMinutes(10),
                UpdatedAt = appointment.ActualEndTime?.AddMinutes(10) ?? appointment.EndAt.AddMinutes(10)
            };

            // Add service as bill item
            var serviceItem = new BillItem
            {
                BillItemId = Guid.NewGuid(),
                BillId = bill.BillId,
                ServiceId = service.ServiceId,
                Type = BillItemType.Service,
                Name = service.Name,
                Quantity = 1,
                Unit = "lần",
                UnitPrice = service.DefaultPrice ?? 0,
                Amount = service.DefaultPrice ?? 0
            };
            billItems.Add(serviceItem);
            bill.Subtotal += serviceItem.Amount;

            // Add some medicines
            var random = new Random(invoiceCounter);
            var medicineCount = random.Next(1, 4);
            for (int i = 0; i < medicineCount && i < clinic1Medicines.Count; i++)
            {
                var medicine = clinic1Medicines[(invoiceCounter + i) % clinic1Medicines.Count];
                var qty = random.Next(10, 30);
                var medicineItem = new BillItem
                {
                    BillItemId = Guid.NewGuid(),
                    BillId = bill.BillId,
                    MedicineId = medicine.MedicineId,
                    Type = BillItemType.Medicine,
                    Name = medicine.Name,
                    Quantity = qty,
                    Unit = medicine.Unit ?? "viên",
                    UnitPrice = medicine.Price ?? 0,
                    Amount = (medicine.Price ?? 0) * qty
                };
                billItems.Add(medicineItem);
                bill.Subtotal += medicineItem.Amount;
            }

            bill.TotalAmount = bill.Subtotal - bill.Discount;
            bill.PaidAmount = bill.TotalAmount;
            bill.ChangeAmount = 0;
            bills.Add(bill);
        }

        // PENDING bill (waiting for payment)
        var pendingPatient = patients.FirstOrDefault(p => p.ClinicId == clinic1.ClinicId);
        if (pendingPatient != null)
        {
            var pendingBill = new Bill
            {
                BillId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                PatientId = pendingPatient.PatientId,
                InvoiceNumber = $"HD-2026-{invoiceCounter++:D4}",
                Status = BillStatus.Pending,
                Subtotal = 350000,
                Discount = 0,
                TotalAmount = 350000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var pendingService = services.FirstOrDefault(s => s.ClinicId == clinic1.ClinicId && s.IsActive);
            if (pendingService != null)
            {
                billItems.Add(new BillItem
                {
                    BillItemId = Guid.NewGuid(),
                    BillId = pendingBill.BillId,
                    ServiceId = pendingService.ServiceId,
                    Type = BillItemType.Service,
                    Name = pendingService.Name,
                    Quantity = 1,
                    Unit = "lần",
                    UnitPrice = pendingService.DefaultPrice ?? 300000,
                    Amount = pendingService.DefaultPrice ?? 300000
                });
            }

            bills.Add(pendingBill);
        }

        // CANCELLED bill (for edge case testing)
        var cancelPatient = patients.Skip(1).FirstOrDefault(p => p.ClinicId == clinic1.ClinicId);
        if (cancelPatient != null)
        {
            bills.Add(new Bill
            {
                BillId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                PatientId = cancelPatient.PatientId,
                InvoiceNumber = $"HD-2026-{invoiceCounter++:D4}",
                Status = BillStatus.Cancelled,
                Subtotal = 200000,
                Discount = 0,
                TotalAmount = 200000,
                Notes = "Hủy theo yêu cầu của bệnh nhân",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            });
        }

        context.Bills.AddRange(bills);
        context.BillItems.AddRange(billItems);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {bills.Count} bills with {billItems.Count} bill items");
    }

    private static async Task SeedPrescriptionTemplatesAsync(
        ClinicDbContext context, List<Clinic> clinics, List<Doctor> doctors, List<Medicine> medicines)
    {
        var templates = new List<PrescriptionTemplate>();
        var templateMedicines = new List<PrescriptionTemplateMedicine>();
        var clinic1 = clinics[0];
        var clinic1Medicines = medicines.Where(m => m.ClinicId == clinic1.ClinicId && m.IsActive).ToList();
        var clinic1Doctors = doctors.Where(d => d.ClinicId == clinic1.ClinicId && d.IsActive).ToList();

        // Shared templates (no specific doctor)
        var sharedTemplates = new[]
        {
            ("Cảm cúm thông thường", "general",
                new[]
                {
                    ("PARA-500", "500mg", 10, "3 lần/ngày sau ăn, khi sốt"), ("VITA-C", "500mg", 15, "1 viên/ngày")
                }),
            ("Viêm họng nhẹ", "general",
                new[] { ("AMOX-500", "500mg", 21, "3 lần/ngày sau ăn"), ("DEXT-15", "15mg", 15, "3 lần/ngày khi ho") }),
            ("Dị ứng da", "dermatology",
                new[]
                {
                    ("LORA-10", "10mg", 10, "1 viên/ngày buổi tối"), ("PRED-5", "5mg", 7, "1 viên/ngày buổi sáng")
                })
        };

        foreach (var (name, category, meds) in sharedTemplates)
        {
            var template = new PrescriptionTemplate
            {
                TemplateId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                DoctorId = null, // Shared
                Name = name,
                Category = category,
                Notes = $"Mẫu đơn thuốc cho {name.ToLower()}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            templates.Add(template);

            foreach (var (medCode, dosage, qty, instructions) in meds)
            {
                var medicine = clinic1Medicines.FirstOrDefault(m => m.Code == medCode);
                if (medicine != null)
                {
                    templateMedicines.Add(new PrescriptionTemplateMedicine
                    {
                        Id = Guid.NewGuid(),
                        TemplateId = template.TemplateId,
                        MedicineId = medicine.MedicineId,
                        Dosage = dosage,
                        Quantity = qty,
                        Instructions = instructions
                    });
                }
            }
        }

        // Doctor-specific template
        var cardioDoctor = clinic1Doctors.FirstOrDefault(d => d.Specialty == "Tim Mạch");
        if (cardioDoctor != null)
        {
            var cardioTemplate = new PrescriptionTemplate
            {
                TemplateId = Guid.NewGuid(),
                ClinicId = clinic1.ClinicId,
                DoctorId = cardioDoctor.DoctorId,
                Name = "Huyết áp cao - Điều trị cơ bản",
                Category = "cardiology",
                Notes = "Mẫu đơn thuốc cho bệnh nhân tăng huyết áp",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            templates.Add(cardioTemplate);

            var amlo = clinic1Medicines.FirstOrDefault(m => m.Code == "AMLO-5");
            if (amlo != null)
            {
                templateMedicines.Add(new PrescriptionTemplateMedicine
                {
                    Id = Guid.NewGuid(),
                    TemplateId = cardioTemplate.TemplateId,
                    MedicineId = amlo.MedicineId,
                    Dosage = "5mg",
                    Quantity = 30,
                    Instructions = "1 viên/ngày buổi sáng"
                });
            }
        }

        context.PrescriptionTemplates.AddRange(templates);
        context.PrescriptionTemplateMedicines.AddRange(templateMedicines);
        await context.SaveChangesAsync();
        Console.WriteLine(
            $"Created {templates.Count} prescription templates with {templateMedicines.Count} medicine items");
    }
}
