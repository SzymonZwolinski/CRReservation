using CRReservation.COMMON.Models;
using System.Net.Http.Json;

namespace CRReservation.COMMON.Services
{
    public interface IClassRoomService
    {
        Task<ClassRoom?> GetClassRoomAsync(int id);
        Task<IEnumerable<ClassRoom>> GetAvailableClassRoomsAsync(DateTime start, DateTime end);
        Task<bool> ReserveClassRoomAsync(Reservation reservation);
    }

    public class ClassRoomService : IClassRoomService
    {
        private readonly HttpClient _httpClient;

        public ClassRoomService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClassRoom?> GetClassRoomAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ClassRoom>($"api/ClassRooms/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<ClassRoom>> GetAvailableClassRoomsAsync(DateTime start, DateTime end)
        {
            try
            {
                // Format dates to ISO 8601 string to ensure correct binding
                // Using "s" format (SortableDateTimePattern) usually works well: 2025-01-15T10:00:00
                string startStr = start.ToString("s");
                string endStr = end.ToString("s");

                var response = await _httpClient.GetFromJsonAsync<IEnumerable<ClassRoom>>($"api/ClassRooms/available?start={startStr}&end={endStr}");
                return response ?? new List<ClassRoom>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching available classrooms: {ex.Message}");
                return new List<ClassRoom>();
            }
        }

        public async Task<bool> ReserveClassRoomAsync(Reservation reservation)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Reservations", reservation);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                return false;
            }
        }
    }
}
