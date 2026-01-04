using CRReservation.COMMON.Enums;
using System.Text.RegularExpressions;

namespace CRReservation.COMMON.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = "Unknown";
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime ReservationDate { get; set; }
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public bool IsRecurring { get; set; }
        public int UserId { get; set; }
        public CRReservation.COMMON.States.User? User { get; set; }
        public string ReservedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "oczekujaca";
        public ClassRoom? ClassRoom { get; set; }
    }
}

