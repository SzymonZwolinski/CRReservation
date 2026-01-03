namespace CRReservation.COMMON.Models
{
	public class ClassRoom
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }
		public bool IsActive { get; set; }
		public string? Notes { get; set; }

        public ClassRoom(){}

        public ClassRoom(
			int id,
			string name,
			int capacity,
			bool isActive,
			string? notes)
		{
			Id = id;
			Name = name;
			Capacity = capacity;
			IsActive = isActive;
			Notes = notes;
		}
	}
}
