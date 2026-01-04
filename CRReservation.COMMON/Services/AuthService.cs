using CRReservation.COMMON.Enums.UserEnums;
using System.Net.Http.Json;
using System.Text.Json;

namespace CRReservation.COMMON.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly UserStateService _userStateService;
    private readonly ITokenService _tokenService;
    private readonly PersistentAuthenticationStateProvider? _authProvider;

    public AuthService(
        HttpClient httpClient,
        UserStateService userStateService,
        ITokenService tokenService,
        PersistentAuthenticationStateProvider? authProvider = null)
    {
        _httpClient = httpClient;
        _userStateService = userStateService;
        _tokenService = tokenService;
        _authProvider = authProvider;
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
                    await _tokenService.SaveTokenAsync(loginResponse.Token);

                    // Ustaw stan użytkownika
                    var role = ParseRole(loginResponse.Role);
                    _userStateService.SetState(true, loginResponse.UserName, role);

                    // Powiadom authentication provider o zalogowaniu
                    if (_authProvider != null)
                    {
                        _authProvider.MarkUserAsAuthenticated(
                            loginResponse.Token,
                            loginResponse.Email,
                            loginResponse.UserName,
                            loginResponse.Role);
                    }

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
        await _tokenService.RemoveTokenAsync();
        _userStateService.SetState(false, "", Role.Student);

        if (_authProvider != null)
        {
            _authProvider.MarkUserAsLoggedOut();
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _tokenService.GetTokenAsync();
    }

    public async Task<LoginResponse> RegisterAsync(string email, string password, string firstName, string lastName, string roleName)
    {
        try
        {
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                RoleName = roleName
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>()
                       ?? new LoginResponse { Success = false, Message = "Błąd deserializacji odpowiedzi" };
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return errorResponse ?? new LoginResponse { Success = false, Message = "Błąd rejestracji" };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = $"Błąd: {ex.Message}" };
        }
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

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? RoleName { get; set; }
}

