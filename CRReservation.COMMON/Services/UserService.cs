using CRReservation.COMMON.Models;
using System.Net.Http.Json;

namespace CRReservation.COMMON.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/Users") ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"api/Users/by-email?email={email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by email {email}: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Users", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Users/{id}", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Users/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user {id}: {ex.Message}");
                return false;
            }
        }
    }
}
