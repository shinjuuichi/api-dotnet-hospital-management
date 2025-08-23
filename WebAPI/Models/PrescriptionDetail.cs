using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

public class PrescriptionDetail : BaseEntity
{
    [Required(ErrorMessage = "Quantity is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Quantity must be between 1 and 50 characters")]
    public string Quantity { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Usage instruction cannot exceed 500 characters")]
    public string? UsageInstruction { get; set; }

    [Required(ErrorMessage = "Medicine ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Medicine ID must be a positive number")]
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "Prescription ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Prescription ID must be a positive number")]
    public int PrescriptionId { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
}
