namespace CRReservation.API.Models;

public class ClassRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
