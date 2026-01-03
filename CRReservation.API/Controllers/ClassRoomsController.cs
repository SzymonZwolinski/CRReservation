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
    //[Authorize]
    public async Task<ActionResult<IEnumerable<ClassRoom>>> GetClassRooms()
    {
        var classRooms = await _context.ClassRooms
            .Where(cr => cr.IsActive)
            .AsNoTracking()
            .ToListAsync();
        
        return Ok(classRooms);
    }

    // GET: api/ClassRooms/5
    [HttpGet("{id}")]
    //[Authorize]
    public async Task<ActionResult<ClassRoom>> GetClassRoom(int id)
    {
        var classRoom = await _context.ClassRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.Id == id && cr.IsActive);

        if (classRoom == null)
        {
            return NotFound(new { error = "Sala nie istnieje" });
        }

        return Ok(classRoom);
    }

    // POST: api/ClassRooms
    [HttpPost]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<ClassRoom>> PostClassRoom(ClassRoom classRoom)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(classRoom.Name))
        {
            return BadRequest(new { error = "Nazwa sali jest wymagana" });
        }

        if (classRoom.Capacity <= 0)
        {
            return BadRequest(new { error = "Pojemność musi być większa niż 0" });
        }

        classRoom.IsActive = true;
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClassRoom), new { id = classRoom.Id }, classRoom);
    }

    // PUT: api/ClassRooms/5
    [HttpPut("{id}")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> PutClassRoom(int id, ClassRoom classRoom)
    {
        if (id != classRoom.Id)
        {
            return BadRequest(new { error = "ID sali nie zgadza się" });
        }

        if (string.IsNullOrWhiteSpace(classRoom.Name))
        {
            return BadRequest(new { error = "Nazwa sali jest wymagana" });
        }

        if (classRoom.Capacity <= 0)
        {
            return BadRequest(new { error = "Pojemność musi być większa niż 0" });
        }

        var existingRoom = await _context.ClassRooms.FindAsync(id);
        if (existingRoom == null)
        {
            return NotFound(new { error = "Sala nie istnieje" });
        }

        existingRoom.Name = classRoom.Name;
        existingRoom.Capacity = classRoom.Capacity;
        existingRoom.Notes = classRoom.Notes;

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

    // DELETE: api/ClassRooms/5
    [HttpDelete("{id}")]
    //[Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteClassRoom(int id)
    {
        var classRoom = await _context.ClassRooms.FindAsync(id);
        if (classRoom == null)
        {
            return NotFound(new { error = "Sala nie istnieje" });
        }

        // Soft delete - mark as inactive instead of deleting
        classRoom.IsActive = false;
        _context.ClassRooms.Update(classRoom);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/ClassRooms/available?start=2025-01-15T10:00&end=2025-01-15T12:00
    [HttpGet("available")]
    //[Authorize]
    public async Task<ActionResult<IEnumerable<ClassRoom>>> GetAvailableClassRooms(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return BadRequest(new { error = "Data rozpoczęcia musi być przed datą zakończenia" });
        }

        var conflictingRoomIds = await _context.Reservations
            .Where(r => r.Status == "potwierdzona" || r.Status == "oczekujaca")
            .Where(r => r.StartDateTime < end && r.EndDateTime > start)
            .Select(r => r.ClassRoomId)
            .Distinct()
            .ToListAsync();

        var availableClassRooms = await _context.ClassRooms
            .Where(cr => cr.IsActive && !conflictingRoomIds.Contains(cr.Id))
            .AsNoTracking()
            .ToListAsync();

        return Ok(availableClassRooms);
    }
}
