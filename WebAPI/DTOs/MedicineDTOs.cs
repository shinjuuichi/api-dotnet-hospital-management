using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class MedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Dosage { get; set; }
    public string? SideEffects { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ModificationDate { get; set; }
}

public class CreateMedicineDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Dosage { get; set; }

    [StringLength(500)]
    public string? SideEffects { get; set; }
}

public class UpdateMedicineDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Dosage { get; set; }

    [StringLength(500)]
    public string? SideEffects { get; set; }
}