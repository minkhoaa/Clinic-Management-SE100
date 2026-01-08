using Microsoft.EntityFrameworkCore;
using ClinicManagement_API.Domains.Entities;
using ClinicManagement_API.Domains.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ClinicManagement_API.Infrastructure.Persisstence;

public class User : IdentityUser<Guid>
{
}

public class Role : IdentityRole<Guid>
{
}

public class ClinicDbContext : IdentityDbContext<User, Role, Guid>
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<DoctorService> DoctorServices => Set<DoctorService>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<AppointmentToken> AppointmentTokens => Set<AppointmentToken>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Patients> Patients => Set<Patients>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalRecordAttachment> MedicalRecordAttachments => Set<MedicalRecordAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Role>().ToTable("Role");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole").HasKey(k => new { k.UserId, k.RoleId });

        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

        modelBuilder.Entity<Clinic>(e =>
        {
            e.ToTable("Clinics");
            e.HasKey(x => x.ClinicId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TimeZone).HasMaxLength(50).HasDefaultValue("Asia/Ho_Chi_Minh");
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(256);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.ToTable("Doctors");
            e.HasKey(x => x.DoctorId);
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Specialty).HasMaxLength(150);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(256);
            e.HasIndex(x => new { x.ClinicId, x.Code }).IsUnique();

            e.HasOne(x => x.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("Services");
            e.HasKey(x => x.ServiceId);
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.ClinicId, x.Code }).IsUnique();

            e.HasOne(x => x.Clinic)
                .WithMany(c => c.Services)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorService>(e =>
        {
            e.ToTable("DoctorServices");
            e.HasKey(x => new { x.ServiceId, x.DoctorId });

            e.HasOne(ds => ds.Service)
                .WithMany(s => s.DoctorServices)
                .HasForeignKey(ds => ds.ServiceId);

            e.HasOne(ds => ds.Doctor)
                .WithMany(p => p.DoctorServices)
                .HasForeignKey(ds => ds.DoctorId);
        });

        modelBuilder.Entity<DoctorAvailability>(e =>
        {
            e.ToTable("DoctorAvailability");
            e.HasKey(x => x.AvailabilityId);
            e.Property(x => x.DayOfWeek).IsRequired();
            e.Property(x => x.StartTime).HasColumnType("time(0)");
            e.Property(x => x.EndTime).HasColumnType("time(0)");
            e.Property(x => x.SlotSizeMin).HasDefaultValue((short)30);
            e.HasIndex(x => new { x.DoctorId, x.DayOfWeek }).HasDatabaseName("IX_Avail_DoctorDow");

            e.HasOne(x => x.Clinic)
                .WithMany(c => c.DoctorAvailabilities)
                .HasForeignKey(x => x.ClinicId);

            e.HasOne(x => x.Doctor)
                .WithMany(p => p.DoctorAvailabilities)
                .HasForeignKey(x => x.DoctorId);
        });

        modelBuilder.Entity<AppointmentToken>(e =>
        {
            e.ToTable("AppointmentTokens");
            e.HasKey(x => x.TokenId);
            e.Property(x => x.Action).HasMaxLength(15).IsRequired();
            e.Property(x => x.Token).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();

            e.HasOne(x => x.Appointment)
                .WithMany(a => a.Tokens)
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("Appointments");
            e.HasKey(x => x.AppointmentId);
            e.Property(x => x.Source).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.ContactFullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.ContactPhone).HasMaxLength(20).IsRequired();
            e.Property(x => x.ContactEmail).HasMaxLength(256);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20)
                .HasDefaultValue(AppointmentStatus.Pending);
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasIndex(x => new { x.ClinicId, x.StartAt, x.EndAt }).HasDatabaseName("IX_Appt_Time");

            e.HasIndex(x => new { x.ClinicId, x.DoctorId, x.StartAt })
                .IsUnique()
                .HasFilter("\"Status\" NOT IN ('Cancelled','NoShow')")
                .HasDatabaseName("UX_Appt_Doctor");

            e.HasOne(x => x.Clinic)
                .WithMany(c => c.Appointments)
                .HasForeignKey(x => x.ClinicId);

            e.HasOne(x => x.Doctor)
                .WithMany(p => p.Appointments)
                .HasForeignKey(x => x.DoctorId);

            e.HasOne(x => x.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(x => x.ServiceId);

            e.HasOne(x => x.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(x => x.PatientId);
        });
        modelBuilder.Entity<Patients>(e =>
        {
            e.ToTable("Patients");
            e.HasKey(x => x.PatientId);
            e.Property(k => k.Gender).HasConversion<string>().HasMaxLength(10);
            e.HasOne(x => x.Clinic)
                .WithMany(k => k.Patients)
                .HasForeignKey(x => x.ClinicId);
        });
        modelBuilder.Entity<StaffUser>(e =>
        {
            e.ToTable("StaffUser");
            e.HasKey(x => x.UserId);
            e.Property(k => k.Role).HasMaxLength(50).HasDefaultValue(AppRoles.Receptionist);
            e.HasOne(x => x.Clinic)
                .WithMany(k => k.StaffUsers)
                .HasForeignKey(x => x.ClinicId);
        });
        modelBuilder.Entity<DoctorTimeOff>(e =>
        {
            e.ToTable("DoctorTimeOff");
            e.HasKey(x => x.TimeOffId);
            e.HasOne(x => x.Doctor)
                .WithMany(k => k.DoctorTimeOffs)
                .HasForeignKey(x => x.DoctorId);
            e.HasOne(x => x.Clinic)
                .WithMany(a => a.DoctorTimeOffs)
                .HasForeignKey(k => k.ClinicId);
        });

        // MedicalRecord configuration
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.ToTable("MedicalRecords");
            e.HasKey(x => x.RecordId);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Diagnosis).HasMaxLength(1000);
            e.Property(x => x.Treatment).HasMaxLength(1000);
            e.Property(x => x.Prescription).HasMaxLength(2000);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => new { x.PatientId, x.RecordDate }).HasDatabaseName("IX_MedicalRecords_PatientDate");

            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // MedicalRecordAttachment configuration
        modelBuilder.Entity<MedicalRecordAttachment>(e =>
        {
            e.ToTable("MedicalRecordAttachments");
            e.HasKey(x => x.AttachmentId);
            e.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            e.Property(x => x.StoredFileName).HasMaxLength(256).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);

            e.HasOne(x => x.MedicalRecord)
                .WithMany(m => m.Attachments)
                .HasForeignKey(x => x.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Bill>(e =>
        {
            e.ToTable("Bills");
            e.HasKey(x => x.BillId);
            e.HasOne(x => x.Clinic)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Patient)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(k => k.Appointment)
                .WithMany(a => a.Bills)
                .HasForeignKey(k => k.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.MedicalRecord)
                .WithMany(k => k.Bills)
                .HasForeignKey(x => x.MedicalRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<BillItem>(e =>
        {
            e.ToTable("BillItems");
            e.HasKey(x => x.BillItemId);
            e.HasOne(x => x.Bill)
                .WithMany(x => x.BillItems)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Service)
                .WithMany(k => k.BillItems)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
