using CRReservation.API.Data;
using CRReservation.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassRoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClassRoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ClassRooms
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ClassRoom>>> GetClassRooms()
    {
        return await _context.ClassRooms.ToListAsync();
    }

    // GET: api/ClassRooms/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ClassRoom>> GetClassRoom(int id)
    {
        var classRoom = await _context.ClassRooms.FindAsync(id);

        if (classRoom == null)
        {
            return NotFound();
        }

        return classRoom;
    }

    // POST: api/ClassRooms
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ClassRoom>> PostClassRoom(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClassRoom), new { id = classRoom.Id }, classRoom);
    }

    // PUT: api/ClassRooms/5
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageRooms")]
    public async Task<IActionResult> PutClassRoom(int id, ClassRoom classRoom)
    {
        if (id != classRoom.Id)
        {
            return BadRequest();
        }

        _context.Entry(classRoom).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClassRoomExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/ClassRooms/5
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageRooms")]
    public async Task<IActionResult> DeleteClassRoom(int id)
    {
        var classRoom = await _context.ClassRooms.FindAsync(id);
        if (classRoom == null)
        {
            return NotFound();
        }

        _context.ClassRooms.Remove(classRoom);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/ClassRooms/available?start=2025-01-15T10:00&end=2025-01-15T12:00
    [HttpGet("available")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ClassRoom>>> GetAvailableClassRooms(DateTime start, DateTime end)
    {
        // Znajdź wszystkie rezerwacje które nachodzą na podany zakres czasu
        var conflictingReservations = await _context.Reservations
            .Where(r => r.Status == "potwierdzona" || r.Status == "oczekujaca") // tylko aktywne rezerwacje
            .Where(r =>
                (r.StartDateTime < end && r.EndDateTime > start) // overlapping
            )
            .Select(r => r.ClassRoomId)
            .Distinct()
            .ToListAsync();

        // Zwróć sale które nie mają konfliktów
        var availableClassRooms = await _context.ClassRooms
            .Where(cr => cr.IsActive && !conflictingReservations.Contains(cr.Id))
            .ToListAsync();

        return availableClassRooms;
    }

    private bool ClassRoomExists(int id)
    {
        return _context.ClassRooms.Any(e => e.Id == id);
    }
}
