using CRReservation.API.Data;
using CRReservation.API.DTOs;
using CRReservation.API.Models;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(ApplicationDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = new LoginResponse();

        try
        {
            // Znajdź użytkownika po email
            var user = await _context.Set<User>()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                response.Success = false;
                response.Message = "Nieprawidłowy email lub hasło";
                return response;
            }

            // Sprawdź hasło (na razie dla seed data używamy plain text, potem będzie hashowane)
            var passwordValid = user.PasswordHash == request.Password ||
                              BCryptNet.Verify(request.Password, user.PasswordHash ?? "");

            if (!passwordValid)
            {
                response.Success = false;
                response.Message = "Nieprawidłowy email lub hasło";
                return response;
            }

            // Generuj token JWT
            var token = _jwtService.GenerateToken(user);

            response.Success = true;
            response.Message = "Zalogowano pomyślnie";
            response.Token = token;
            response.Role = user.RoleName;
            response.UserName = $"{user.FirstName} {user.LastName}";
            response.Email = user.Email;
            response.Expiration = DateTime.Now.AddHours(24);

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"Błąd podczas logowania: {ex.Message}";
            return response;
        }
    }

    public async Task<User?> RegisterAsync(string email, string password, string firstName, string lastName, string roleName)
    {
        try
        {
            // Sprawdź czy użytkownik już istnieje
            var existingUser = await _context.Set<User>().FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (existingUser != null)
            {
                return null; // Użytkownik już istnieje
            }

            // Sprawdź czy rola istnieje
            var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return null; // Nieprawidłowa rola
            }

            // Hashuj hasło
            var hashedPassword = BCryptNet.HashPassword(password);

            var user = new User
            {
                UserName = email, // Identity wymaga UserName
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                RoleName = roleName,
                PasswordHash = hashedPassword,
                EmailConfirmed = true, // Dla uproszczenia w development
                SecurityStamp = Guid.NewGuid().ToString()
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
        catch
        {
            return null;
        }
    }
}
