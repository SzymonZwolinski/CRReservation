using CRReservation.API.Data;
using CRReservation.API.DTOs;
using CRReservation.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Reservations
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
    {
        var reservations = await _context.Reservations
            .Include(r => r.ClassRoom)
            .ToListAsync();

        var users = await _context.Users.ToDictionaryAsync(u => u.Id);
        var groups = await _context.Groups.ToDictionaryAsync(g => g.Id);

        var reservationDtos = reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            Status = r.Status,
            ClassRoomId = r.ClassRoomId,
            ClassRoomName = r.ClassRoom?.Name ?? "Unknown",
            ReservationDate = r.ReservationDate,
            GroupId = r.GroupId,
            GroupName = r.GroupId.HasValue && groups.ContainsKey(r.GroupId.Value) ? groups[r.GroupId.Value].Name : null,
            IsRecurring = r.IsRecurring,
            StartDateTime = r.StartDateTime,
            EndDateTime = r.EndDateTime,
            UserId = r.UserId,
            UserName = users.ContainsKey(r.UserId) ? $"{users[r.UserId].FirstName} {users[r.UserId].LastName}" : "Unknown"
        }).ToList();

        return reservationDtos;
    }

    // GET: api/Reservations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> GetReservation(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.ClassRoom)
            .Include(r => r.User)
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        return reservation;
    }

    // POST: api/Reservations
    [HttpPost]
    [Authorize(Policy = "CanCreateReservation")]
    public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    // PUT: api/Reservations/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutReservation(int id, Reservation reservation)
    {
        if (id != reservation.Id)
        {
            return BadRequest();
        }

        _context.Entry(reservation).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReservationExists(id))
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

    // DELETE: api/Reservations/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservations.Any(e => e.Id == id);
    }
}
