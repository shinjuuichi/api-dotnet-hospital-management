using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Specialty;

public class CreateSpecialtyRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateSpecialtyRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

public class SpecialtyResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public int DoctorCount { get; set; }
}
