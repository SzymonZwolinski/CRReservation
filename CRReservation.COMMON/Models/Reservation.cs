using CRReservation.COMMON.Enums;

namespace CRReservation.COMMON.Models
{
	public class Reservation
	{
		public int Id { get; set; }
		public int ClassRoomId { get; set; }
		public string ClassRoomName { get; set; } = "Unknown";
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string ReservedBy { get; set; }
		public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
	}
}
