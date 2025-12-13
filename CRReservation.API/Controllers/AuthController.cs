using CRReservation.API.DTOs;
using CRReservation.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Email i hasło są wymagane"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Email i hasło nie mogą być puste"
            });
        }

        var response = await _authService.LoginAsync(request);
        
        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Nieprawidłowe dane rejestracji"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Email, hasło i imię są wymagane"
            });
        }

        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(request.Email))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Nieprawidłowy format adresu email"
            });
        }

        var user = await _authService.RegisterAsync(
            request.Email, 
            request.Password, 
            request.FirstName, 
            request.LastName ?? "",
            request.RoleName ?? "student"
        );

        if (user == null)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "Rejestracja nie powiodła się. Użytkownik może już istnieć lub rola nie istnieje."
            });
        }

        return StatusCode(201, new LoginResponse
        {
            Success = true,
            Message = "Rejestracja powiodła się. Możesz się teraz zalogować.",
            Email = user.Email,
            UserName = $"{user.FirstName} {user.LastName}"
        });
    }
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Imię jest wymagane")]
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? RoleName { get; set; }
}
