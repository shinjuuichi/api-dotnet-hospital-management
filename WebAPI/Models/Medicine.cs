using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
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

    public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; } = new List<PrescriptionDetail>();
}
