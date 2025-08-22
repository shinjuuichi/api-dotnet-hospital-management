using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class SpecialtyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class CreateSpecialtyDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateSpecialtyDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}