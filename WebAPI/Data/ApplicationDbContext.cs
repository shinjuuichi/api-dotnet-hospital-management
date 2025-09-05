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

        modelBuilder.Entity<Medicine>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Prescription>()
            .Property(p => p.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PrescriptionDetail>()
            .Property(pd => pd.UnitPrice)
            .HasPrecision(18, 2);

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

