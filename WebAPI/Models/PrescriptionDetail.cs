using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models;

public class PrescriptionDetail
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.00", "79228162514264337593543950335", ErrorMessage = "Unit price must be non-negative")]
    public decimal UnitPrice { get; set; }

    [StringLength(500, ErrorMessage = "Usage instruction cannot exceed 500 characters")]
    public string? UsageInstruction { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Medicine ID must be a positive number")]
    public int MedicineId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Prescription ID must be a positive number")]
    public int PrescriptionId { get; set; }

    public Prescription Prescription { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;

    [NotMapped]
    public decimal TotalPrice => UnitPrice * Quantity;
}
