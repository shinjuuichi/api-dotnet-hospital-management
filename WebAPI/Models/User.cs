using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

[Index(nameof(Email), IsUnique = true, Name = "IX_Users_Email_Unique")]
[Index(nameof(PhoneNumber), IsUnique = true, Name = "IX_Users_PhoneNumber_Unique")]
public class User : AuditableEntity
{
    public string? Avatar { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(GenderEnum), ErrorMessage = "Invalid gender selected")]
    public GenderEnum Gender { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [EnumDataType(typeof(RoleEnum), ErrorMessage = "Invalid role selected")]
    public RoleEnum Role { get; set; }

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
