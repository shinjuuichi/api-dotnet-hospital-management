using WebAPI.Models.EntityAbstractions;
using WebAPI.Models.Enum;

namespace WebAPI.Models;

public class User : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public RoleEnum Role { get; set; }

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
