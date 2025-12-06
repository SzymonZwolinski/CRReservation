using Microsoft.AspNetCore.Identity;

namespace CRReservation.API.Models;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Foreign key - role name for our custom role system
    public string RoleName { get; set; } = string.Empty;

    // Navigation properties
    public Role? Role { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}