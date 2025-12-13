using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace CRReservation.COMMON.Services;

public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService _tokenService;
    private readonly UserStateService _userStateService;
    private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

    public PersistentAuthenticationStateProvider(ITokenService tokenService, UserStateService userStateService)
    {
        _tokenService = tokenService;
        _userStateService = userStateService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenService.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(_cachedUser);
            }

            // Zdekoduj token JWT aby uzyskaæ claims
            var claims = ParseJwtClaims(token);
            
            if (claims.Count > 0)
            {
                var identity = new ClaimsIdentity(claims, "jwt");
                _cachedUser = new ClaimsPrincipal(identity);
                
                // Aktualizuj stan u¿ytkownika
                var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                _userStateService.SetState(true, _cachedUser.FindFirst(ClaimTypes.Name)?.Value ?? "", 
                    string.IsNullOrEmpty(roleClaim) ? Enums.UserEnums.Role.Student : ParseRole(roleClaim));
            }
            else
            {
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
                await _tokenService.RemoveTokenAsync();
            }

            return new AuthenticationState(_cachedUser);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetAuthenticationStateAsync: {ex.Message}");
            _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(_cachedUser);
        }
    }

    public void MarkUserAsAuthenticated(string token, string email, string userName, string role)
    {
        var claims = ParseJwtClaims(token);
        
        if (claims.Count == 0)
        {
            claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        _cachedUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    public void MarkUserAsLoggedOut()
    {
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    private List<Claim> ParseJwtClaims(string token)
    {
        try
        {
            var claims = new List<Claim>();
            var parts = token.Split('.');
            
            if (parts.Length != 3)
                return claims;

            var decoded = DecodeBase64Url(parts[1]);
            var jsonDoc = System.Text.Json.JsonDocument.Parse(decoded);
            
            foreach (var property in jsonDoc.RootElement.EnumerateObject())
            {
                var claimValue = property.Value.ToString();
                
                // Mapy standardowych JWT claim types
                var claimType = property.Name switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "email" => ClaimTypes.Email,
                    "name" => ClaimTypes.Name,
                    "role" => ClaimTypes.Role,
                    _ => property.Name
                };
                
                claims.Add(new Claim(claimType, claimValue));
            }

            return claims;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing JWT: {ex.Message}");
            return new List<Claim>();
        }
    }

    private string DecodeBase64Url(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2:
                output += "==";
                break;
            case 3:
                output += "=";
                break;
        }

        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(output));
    }

    private Enums.UserEnums.Role ParseRole(string roleName)
    {
        return roleName.ToLower() switch
        {
            "admin" => Enums.UserEnums.Role.Administrator,
            "administrator" => Enums.UserEnums.Role.Administrator,
            "prowadzacy" => Enums.UserEnums.Role.Prowadz¹cy,
            "prowadz¹cy" => Enums.UserEnums.Role.Prowadz¹cy,
            "student" => Enums.UserEnums.Role.Student,
            _ => Enums.UserEnums.Role.Student
        };
    }
}
