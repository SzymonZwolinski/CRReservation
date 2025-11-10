namespace CRReservation.COMMON.Models
{
	public class Reservation
	{
		public int Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string ReservedBy { get; set; }

	}
}
