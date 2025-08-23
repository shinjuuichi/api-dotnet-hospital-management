using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Models.Enum;

namespace WebAPI.Data.Seeds
{
    public class UserSeed
    {
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Specialty>().HasData(
                new Specialty
                {
                    Id = 1,
                    Name = "Cardiology",
                    CreationDate = DateTime.UtcNow
                },
                new Specialty
                {
                    Id = 2,
                    Name = "Neurology",
                    CreationDate = DateTime.UtcNow
                },
                new Specialty
                {
                    Id = 3,
                    Name = "Pediatrics",
                    CreationDate = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "John Manager",
                    Email = "manager@hospital.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
                    Role = RoleEnum.Manager,
                    IsVerified = true,
                    CreationDate = DateTime.UtcNow
                },
                new User
                {
                    Id = 2,
                    FullName = "Dr. Sarah Wilson",
                    Email = "sarah.wilson@hospital.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
                    Role = RoleEnum.Doctor,
                    IsVerified = true,
                    CreationDate = DateTime.UtcNow
                },
                new User
                {
                    Id = 3,
                    FullName = "Alice Johnson",
                    Email = "alice.johnson@email.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
                    Role = RoleEnum.Customer,
                    IsVerified = true,
                    CreationDate = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    LicenseNumber = "MD-001-2023",
                    PhoneNumber = "+1-555-0101",
                    Gender = GenderEnum.Female,
                    DateOfBirth = new DateTime(1980, 5, 15),
                    UserId = 2,
                    SpecialtyId = 1,
                    CreationDate = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    PhoneNumber = "+1-555-0201",
                    DateOfBirth = new DateTime(1990, 3, 10),
                    Gender = GenderEnum.Female,
                    Address = "123 Main Street, Anytown, ST 12345",
                    UserId = 3,
                    CreationDate = DateTime.UtcNow
                }
            );
        }
    }
}
