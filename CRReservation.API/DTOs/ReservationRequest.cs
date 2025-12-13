namespace CRReservation.API.DTOs;

public class CreateReservationRequest
{
    public int ClassRoomId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int? GroupId { get; set; }
    public bool IsRecurring { get; set; } = false;
}

public class UpdateReservationRequest
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int? GroupId { get; set; }
    public bool IsRecurring { get; set; } = false;
}
