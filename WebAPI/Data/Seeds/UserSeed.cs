using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Models.Enum;
using WebAPI.Utils;

namespace WebAPI.Data.Seeds
{
    public class UserSeed
    {
        public static void SeedUsers(ModelBuilder modelBuilder)
        {
            var seededAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Specialty>().HasData(
                new Specialty { Id = 1, Name = "Cardiology", CreationDate = seededAt, IsDeleted = false },
                new Specialty { Id = 2, Name = "Neurology", CreationDate = seededAt, IsDeleted = false },
                new Specialty { Id = 3, Name = "Pediatrics", CreationDate = seededAt, IsDeleted = false }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "John Manager",
                    Email = "manager@gmail.com",
                    Password = CryptoPassword.EncryptPassword("Admin@123"),
                    PhoneNumber = "0900000001",
                    DateOfBirth = new DateTime(1980, 5, 12),
                    Gender = GenderEnum.Male,
                    Role = RoleEnum.Manager,
                    IsVerified = true,
                    CreationDate = seededAt,
                },
                new User
                {
                    Id = 2,
                    FullName = "Dr. Sarah Wilson",
                    Email = "sarah.wilson@hospital.com",
                    Password = CryptoPassword.EncryptPassword("Admin@123"),
                    PhoneNumber = "0900000002",
                    DateOfBirth = new DateTime(1985, 6, 15),
                    Gender = GenderEnum.Female,
                    Role = RoleEnum.Doctor,
                    IsVerified = true,
                    CreationDate = seededAt,
                },
                new User
                {
                    Id = 3,
                    FullName = "Alice Johnson",
                    Email = "alice.johnson@email.com",
                    Password = CryptoPassword.EncryptPassword("Admin@123"),
                    PhoneNumber = "0900000003",
                    DateOfBirth = new DateTime(1995, 3, 20),
                    Gender = GenderEnum.Female,
                    Role = RoleEnum.Customer,
                    IsVerified = true,
                    CreationDate = seededAt,
                }
            );

            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    LicenseNo = "MD-001-2023",
                    Bio = "Experienced cardiologist with 10+ years of practice",
                    YearOfExperience = 10,
                    UserId = 2,
                    SpecialtyId = 1,
                    CreationDate = seededAt,
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    InsuranceNo = "INS-123456",
                    Address = "123 Main Street, Anytown, ST 12345",
                    UserId = 3,
                    CreationDate = seededAt,
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<Medicine>().HasData(
                new Medicine
                {
                    Id = 1,
                    Name = "Paracetamol",
                    Description = "Pain reliever and fever reducer",
                    Price = 15.50m,
                    CreationDate = seededAt,
                    IsDeleted = false
                },
                new Medicine
                {
                    Id = 2,
                    Name = "Ibuprofen",
                    Description = "Nonsteroidal anti-inflammatory drug (NSAID)",
                    Price = 12.75m,
                    CreationDate = seededAt,
                    IsDeleted = false
                },
                new Medicine
                {
                    Id = 3,
                    Name = "Amoxicillin",
                    Description = "Antibiotic used to treat bacterial infections",
                    Price = 25.00m,
                    CreationDate = seededAt,
                    IsDeleted = false
                }
            );
        }
    }
}