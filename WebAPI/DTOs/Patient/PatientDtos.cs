namespace WebAPI.DTOs.Patient;

public class PatientResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string? Avatar { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? InsuranceNo { get; set; }
    public DateTime CreationDate { get; set; }
}

public class PatientListResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? InsuranceNo { get; set; }
}
