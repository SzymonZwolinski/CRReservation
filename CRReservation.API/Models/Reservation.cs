namespace CRReservation.API.Models;

public class Reservation
{
    public int Id { get; set; }
    public string Status { get; set; } = "oczekujaca"; // pending, confirmed, cancelled
    public int ClassRoomId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int? GroupId { get; set; }
    public bool IsRecurring { get; set; } = false;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public ClassRoom? ClassRoom { get; set; }
    public Group? Group { get; set; }
    public User? User { get; set; }
}