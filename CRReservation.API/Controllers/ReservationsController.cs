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
            .Include(r => r.User)
            .Include(r => r.Group)
            .ToListAsync();

        var reservationDtos = reservations.Select(r => r.ToDto()).ToList();
        return Ok(reservationDtos);
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
    [Authorize]
    public async Task<ActionResult<ReservationDto>> PostReservation([FromBody] CreateReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate dates
        if (request.StartDateTime >= request.EndDateTime)
        {
            return BadRequest(new { error = "Data zakończenia musi być po dacie rozpoczęcia" });
        }

        // Check if classroom exists
        var classroom = await _context.ClassRooms.FindAsync(request.ClassRoomId);
        if (classroom == null)
        {
            return NotFound(new { error = "Sala nie istnieje" });
        }

        // Check if classroom is available
        var isAvailable = await IsClassRoomAvailableAsync(request.ClassRoomId, request.StartDateTime, request.EndDateTime);
        if (!isAvailable)
        {
            return BadRequest(new { error = "Sala jest zajęta w wybranym terminie" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            return Unauthorized(new { error = "Nie można określić użytkownika" });
        }

        var reservation = new Reservation
        {
            ClassRoomId = request.ClassRoomId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            GroupId = request.GroupId,
            IsRecurring = request.IsRecurring,
            UserId = userId,
            Status = "oczekujaca",
            ReservationDate = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        await _context.Entry(reservation)
            .Reference(r => r.ClassRoom)
            .LoadAsync();
        await _context.Entry(reservation)
            .Reference(r => r.User)
            .LoadAsync();

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation.ToDto());
    }

    // PUT: api/Reservations/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutReservation(int id, [FromBody] UpdateReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.StartDateTime >= request.EndDateTime)
        {
            return BadRequest(new { error = "Data zakończenia musi być po dacie rozpoczęcia" });
        }

        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound(new { error = "Rezerwacja nie istnieje" });
        }

        if (reservation.Status != "oczekujaca")
        {
            return BadRequest(new { error = "Można edytować tylko rezerwacje oczekujące" });
        }

        var isAvailable = await IsClassRoomAvailableAsync(reservation.ClassRoomId, request.StartDateTime, request.EndDateTime, id);
        if (!isAvailable)
        {
            return BadRequest(new { error = "Sala jest zajęta w wybranym terminie" });
        }

        reservation.StartDateTime = request.StartDateTime;
        reservation.EndDateTime = request.EndDateTime;
        reservation.GroupId = request.GroupId;
        reservation.IsRecurring = request.IsRecurring;

        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

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
            return NotFound(new { error = "Rezerwacja nie istnieje" });
        }

        if (reservation.Status != "oczekujaca")
        {
            return BadRequest(new { error = "Można usuwać tylko rezerwacje oczekujące" });
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/Reservations/filter?startDate=2025-01-01&endDate=2025-01-31&userId=1&status=potwierdzona
    [HttpGet("filter")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetFilteredReservations(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        string? status = null)
    {
        var query = _context.Reservations.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(r => r.StartDateTime.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.EndDateTime.Date <= endDate.Value.Date);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status.ToLower() == status.ToLower());
        }

        var reservations = await query
            .Include(r => r.ClassRoom)
            .Include(r => r.User)
            .Include(r => r.Group)
            .ToListAsync();

        var reservationDtos = reservations.Select(r => r.ToDto()).ToList();
        return Ok(reservationDtos);
    }

    // PUT: api/Reservations/5/approve
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ApproveReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound(new { error = "Rezerwacja nie istnieje" });
        }

        if (reservation.Status != "oczekujaca")
        {
            return BadRequest(new { error = "Można zatwierdzić tylko rezerwacje oczekujące" });
        }

        reservation.Status = "potwierdzona";
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Reservations/5/reject
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RejectReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound(new { error = "Rezerwacja nie istnieje" });
        }

        if (reservation.Status != "oczekujaca")
        {
            return BadRequest(new { error = "Można odrzucić tylko rezerwacje oczekujące" });
        }

        reservation.Status = "odrzucona";
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Reservations/5/revoke
    [HttpPut("{id}/revoke")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RevokeReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound(new { error = "Rezerwacja nie istnieje" });
        }

        if (reservation.Status == "anulowana")
        {
            return BadRequest(new { error = "Rezerwacja jest już anulowana" });
        }

        reservation.Status = "anulowana";
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> IsClassRoomAvailableAsync(int classRoomId, DateTime start, DateTime end, int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(r => r.ClassRoomId == classRoomId)
            .Where(r => r.Status == "potwierdzona" || r.Status == "oczekujaca")
            .Where(r => r.StartDateTime < end && r.EndDateTime > start);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        var hasConflict = await query.AnyAsync();
        return !hasConflict;
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservations.Any(e => e.Id == id);
    }
}
