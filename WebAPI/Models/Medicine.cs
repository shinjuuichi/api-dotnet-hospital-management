using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.EntityAbstractions;

namespace WebAPI.Models;

[Index(nameof(Name), IsUnique = true, Name = "IX_Medicines_Name_Unique")]
public class Medicine : AuditableEntity
{
    [Required(ErrorMessage = "Medicine name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Medicine name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.00", "79228162514264337593543950335", ErrorMessage = "Unit price must be non-negative")]
    public decimal Price { get; set; }

    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
