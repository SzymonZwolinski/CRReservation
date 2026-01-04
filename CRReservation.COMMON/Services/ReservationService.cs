using CRReservation.COMMON.Models;
using System.Net.Http.Json;

namespace CRReservation.COMMON.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetReservationsAsync();
        Task<ReservationDto?> GetReservationAsync(int id);
        Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(int userId);
        Task<IEnumerable<ReservationDto>> GetFilteredReservationsAsync(DateTime? start = null, DateTime? end = null, int? userId = null, string? status = null);
        Task<bool> CreateReservationAsync(CreateReservationRequest request);
        Task<bool> UpdateReservationAsync(int id, UpdateReservationRequest request);
        Task<bool> DeleteReservationAsync(int id);
        Task<bool> ApproveReservationAsync(int id);
        Task<bool> RejectReservationAsync(int id);
        Task<bool> RevokeReservationAsync(int id);
    }

    public class ReservationService : IReservationService
    {
        private readonly HttpClient _httpClient;

        public ReservationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ReservationDto>> GetReservationsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationDto>>("api/Reservations") ?? new List<ReservationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reservations: {ex.Message}");
                return new List<ReservationDto>();
            }
        }

        public async Task<ReservationDto?> GetReservationAsync(int id)
        {
            try
            {
                var reservation = await _httpClient.GetFromJsonAsync<Reservation>($"api/Reservations/{id}");
                if (reservation == null) return null;

                // Manual mapping because API returns Entity, interface expects DTO
                return new ReservationDto
                {
                    Id = reservation.Id,
                    Status = reservation.Status,
                    ClassRoomId = reservation.ClassRoomId,
                    ClassRoomName = reservation.ClassRoom?.Name ?? "",
                    ReservationDate = reservation.ReservationDate,
                    GroupId = reservation.GroupId,
                    GroupName = reservation.Group?.Name,
                    IsRecurring = reservation.IsRecurring,
                    StartDateTime = reservation.StartDateTime,
                    EndDateTime = reservation.EndDateTime,
                    UserId = reservation.UserId,
                    UserName = reservation.User != null ? $"{reservation.User.UserName} " : ""
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reservation {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(int userId)
        {
            try
            {
                // Using the filter endpoint to get reservations for a specific user
                return await GetFilteredReservationsAsync(userId: userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting my reservations: {ex.Message}");
                return new List<ReservationDto>();
            }
        }

        public async Task<IEnumerable<ReservationDto>> GetFilteredReservationsAsync(DateTime? start = null, DateTime? end = null, int? userId = null, string? status = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (start.HasValue) queryParams.Add($"startDate={start.Value:yyyy-MM-dd}");
                if (end.HasValue) queryParams.Add($"endDate={end.Value:yyyy-MM-dd}");
                if (userId.HasValue) queryParams.Add($"userId={userId.Value}");
                if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

                return await _httpClient.GetFromJsonAsync<IEnumerable<ReservationDto>>($"api/Reservations/filter{queryString}") ?? new List<ReservationDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered reservations: {ex.Message}");
                return new List<ReservationDto>();
            }
        }

        public async Task<bool> CreateReservationAsync(CreateReservationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Reservations", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateReservationAsync(int id, UpdateReservationRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Reservations/{id}", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating reservation {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReservationAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Reservations/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting reservation {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ApproveReservationAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/Reservations/{id}/approve", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving reservation {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectReservationAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/Reservations/{id}/reject", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting reservation {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokeReservationAsync(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/Reservations/{id}/revoke", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error revoking reservation {id}: {ex.Message}");
                return false;
            }
        }
    }
}
