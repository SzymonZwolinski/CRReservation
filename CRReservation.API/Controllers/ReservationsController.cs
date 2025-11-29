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
        try
        {
            var reservations = await _context.Reservations.ToListAsync();

            var reservationDtos = reservations.Select(r => new ReservationDto
            {
                Id = r.Id,
                Status = r.Status,
                ClassRoomId = r.ClassRoomId,
                ClassRoomName = "Test", // Uproszczono
                ReservationDate = r.ReservationDate,
                GroupId = r.GroupId,
                GroupName = null, // Uproszczono
                IsRecurring = r.IsRecurring,
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                UserId = r.UserId,
                UserName = "Test User" // Uproszczono
            }).ToList();

            return reservationDtos;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
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
    public async Task<IActionResult> PostReservation([FromBody] object data)
    {
        return Ok(new { message = "POST działa!" });
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

        // Filtruj po dacie rozpoczęcia
        if (startDate.HasValue)
        {
            query = query.Where(r => r.StartDateTime.Date >= startDate.Value.Date);
        }

        // Filtruj po dacie zakończenia
        if (endDate.HasValue)
        {
            query = query.Where(r => r.EndDateTime.Date <= endDate.Value.Date);
        }

        // Filtruj po użytkowniku
        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        // Filtruj po statusie
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(r => r.Status.ToLower() == status.ToLower());
        }

        var reservations = await query
            .Include(r => r.ClassRoom)
            .Include(r => r.User)
            .Include(r => r.Group)
            .ToListAsync();

        var reservationDtos = reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            Status = r.Status,
            ClassRoomId = r.ClassRoomId,
            ClassRoomName = r.ClassRoom?.Name ?? "Unknown",
            ReservationDate = r.ReservationDate,
            GroupId = r.GroupId,
            GroupName = r.GroupId.HasValue && r.Group != null ? r.Group.Name : null,
            IsRecurring = r.IsRecurring,
            StartDateTime = r.StartDateTime,
            EndDateTime = r.EndDateTime,
            UserId = r.UserId,
            UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown"
        }).ToList();

        return reservationDtos;
    }

    // PUT: api/Reservations/5/approve
    [HttpPut("{id}/approve")]
    [Authorize(Policy = "CanApproveReservation")]
    public async Task<IActionResult> ApproveReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Status = "potwierdzona";
        await _context.SaveChangesAsync();

        // TODO: Wysłać email powiadomienia do użytkownika

        return NoContent();
    }

    // PUT: api/Reservations/5/reject
    [HttpPut("{id}/reject")]
    [Authorize(Policy = "CanApproveReservation")]
    public async Task<IActionResult> RejectReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Status = "odrzucona";
        await _context.SaveChangesAsync();

        // TODO: Wysłać email powiadomienia do użytkownika

        return NoContent();
    }

    // PUT: api/Reservations/5/revoke
    [HttpPut("{id}/revoke")]
    [Authorize(Policy = "CanApproveReservation")]
    public async Task<IActionResult> RevokeReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Status = "anulowana";
        await _context.SaveChangesAsync();

        // TODO: Wysłać email powiadomienia do użytkownika

        return NoContent();
    }

    private async Task<bool> IsClassRoomAvailable(int classRoomId, DateTime start, DateTime end)
    {
        // Sprawdź czy są konflikty z istniejącymi rezerwacjami
        var conflictingReservations = await _context.Reservations
            .Where(r => r.ClassRoomId == classRoomId)
            .Where(r => r.Status == "potwierdzona" || r.Status == "oczekujaca")
            .Where(r => r.StartDateTime < end && r.EndDateTime > start)
            .AnyAsync();

        return !conflictingReservations;
    }

    private bool ReservationExists(int id)
    {
        return _context.Reservations.Any(e => e.Id == id);
    }
}
