using CRReservation.COMMON.Enums.UserEnums;
using CRReservation.COMMON.Models;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace CRReservation.COMMON.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly UserStateService _userStateService;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(HttpClient httpClient, UserStateService userStateService, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _userStateService = userStateService;
        _jsRuntime = jsRuntime;
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
                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
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
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveTokenAsync(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
    }

    private async Task RemoveTokenAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
    }

    private Role ParseRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) return Role.Student;

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
