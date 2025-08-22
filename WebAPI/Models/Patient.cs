using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

public class Patient : AuditableEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public GenderEnum Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public int UserId { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
