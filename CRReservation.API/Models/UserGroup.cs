namespace CRReservation.API.Models;

public class UserGroup
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public Group? Group { get; set; }
    public User? User { get; set; }
}