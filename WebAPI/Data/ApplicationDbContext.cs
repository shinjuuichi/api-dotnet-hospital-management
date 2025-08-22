using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Seeds;
using WebAPI.Models;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Specialty)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Appointment)
            .WithOne(a => a.Prescription)
            .HasForeignKey<Prescription>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Specialty)
            .WithMany(s => s.Doctors)
            .HasForeignKey(d => d.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(pd => pd.Prescription)
            .WithMany(p => p.PrescriptionDetails)
            .HasForeignKey(pd => pd.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrescriptionDetail>()
            .HasOne(pd => pd.Medicine)
            .WithMany(m => m.PrescriptionDetails)
            .HasForeignKey(pd => pd.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);

        UserSeed.SeedUsers(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreationDate = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModificationDate = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

