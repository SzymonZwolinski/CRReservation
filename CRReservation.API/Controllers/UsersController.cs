using CRReservation.API.Data;
using CRReservation.API.DTOs;
using CRReservation.API.Extensions;
using CRReservation.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .ToListAsync();

        var userDtos = users.Select(u => u.ToDto()).ToList();
        return Ok(userDtos);
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    //[Authorize]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { error = "U¿ytkownik nie istnieje" });
        }

        return Ok(user.ToDto());
    }

    // GET: api/Users/by-email?email=user@example.com
    [HttpGet("by-email")]
    //[Authorize]
    public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email jest wymagany" });
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return NotFound(new { error = "U¿ytkownik nie istnieje" });
        }

        return Ok(user.ToDto());
    }

    // POST: api/Users
    [HttpPost]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<UserDto>> PostUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName))
        {
            return BadRequest(new { error = "Email i imiê s¹ wymagane" });
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { error = "U¿ytkownik z tym emailem ju¿ istnieje" });
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleName = request.RoleName ?? "student",
            EmailConfirmed = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user.ToDto());
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> PutUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "U¿ytkownik nie istnieje" });
        }

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;

        if (!string.IsNullOrEmpty(request.RoleName))
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Name == request.RoleName);
            if (!roleExists)
            {
                return BadRequest(new { error = "Rola nie istnieje" });
            }
            user.RoleName = request.RoleName;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "U¿ytkownik nie istnieje" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? RoleName { get; set; }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RoleName { get; set; }
}
