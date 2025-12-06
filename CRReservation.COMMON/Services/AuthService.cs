using CRReservation.COMMON.Enums.UserEnums;
using System.Net.Http.Json;
using System.Text.Json;

namespace CRReservation.COMMON.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly UserStateService _userStateService;

    public AuthService(HttpClient httpClient, UserStateService userStateService)
    {
        _httpClient = httpClient;
        _userStateService = userStateService;
    }

    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse?.Success == true && loginResponse.Token != null)
                {
                    // Zapisz token w localStorage
                    await SaveTokenAsync(loginResponse.Token);

                    // Ustaw stan użytkownika
                    var role = ParseRole(loginResponse.Role);
                    _userStateService.SetState(true, loginResponse.UserName, role);

                    return loginResponse;
                }
            }

            return new LoginResponse { Success = false, Message = "Błąd logowania" };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = $"Błąd: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        await RemoveTokenAsync();
        _userStateService.SetState(false, "", Role.Student);
    }

    public async Task<string?> GetTokenAsync()
    {
        // W Blazor można użyć JS interop do localStorage
        // Na razie zwraca null - trzeba zaimplementować
        return null;
    }

    private async Task SaveTokenAsync(string token)
    {
        // Implementacja localStorage przez JS interop
        await Task.CompletedTask;
    }

    private async Task RemoveTokenAsync()
    {
        // Implementacja usunięcia z localStorage
        await Task.CompletedTask;
    }

    private Role ParseRole(string roleName)
    {
        return roleName.ToLower() switch
        {
            "admin" => Role.Administrator,
            "administrator" => Role.Administrator,
            "prowadzacy" => Role.Prowadzący,
            "prowadzący" => Role.Prowadzący,
            "student" => Role.Student,
            _ => Role.Student
        };
    }
}

// DTOs dla API
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
