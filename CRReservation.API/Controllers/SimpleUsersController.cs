using CRReservation.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/simple-users")]
public class SimpleUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SimpleUsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var count = await _context.Users.CountAsync();
            return Ok(new { message = $"Znaleziono {count} użytkowników" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
