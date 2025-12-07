using CRReservation.API.Services;
using CRReservation.COMMON.Models;
using Microsoft.AspNetCore.Mvc;

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
                Message = "Nieprawidłowe dane logowania"
            });
        }

        var response = await _authService.LoginAsync(request);
        return response;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        // To będzie wymagało osobnego DTO dla rejestracji
        // Na razie zwróć NotImplemented
        return StatusCode(501, new LoginResponse
        {
            Success = false,
            Message = "Rejestracja nie została jeszcze zaimplementowana"
        });
    }
}

