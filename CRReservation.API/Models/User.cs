namespace CRReservation.API.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Foreign key
    public string RoleName { get; set; } = string.Empty;

    // Navigation properties
    public Role? Role { get; set; }
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}