namespace CRReservation.COMMON.Models
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

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
}
