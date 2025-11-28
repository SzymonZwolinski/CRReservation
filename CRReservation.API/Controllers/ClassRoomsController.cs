using CRReservation.API.Data;
using CRReservation.API.Models;
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
    public async Task<ActionResult<ClassRoom>> PostClassRoom(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClassRoom), new { id = classRoom.Id }, classRoom);
    }

    // PUT: api/ClassRooms/5
    [HttpPut("{id}")]
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

    private bool ClassRoomExists(int id)
    {
        return _context.ClassRooms.Any(e => e.Id == id);
    }
}
