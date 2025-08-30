using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Medicine;

public class CreateMedicineRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
    public decimal Price { get; set; }
}

public class UpdateMedicineRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
    public decimal Price { get; set; }
}

public class MedicineResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime CreationDate { get; set; }
}
