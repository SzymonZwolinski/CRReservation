using CRReservation.API.DTOs;
using CRReservation.API.Models;

namespace CRReservation.API.Extensions;

public static class MappingExtensions
{
    public static ReservationDto ToDto(this Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            Status = reservation.Status,
            ClassRoomId = reservation.ClassRoomId,
            ClassRoomName = reservation.ClassRoom?.Name ?? "Unknown",
            ReservationDate = reservation.ReservationDate,
            GroupId = reservation.GroupId,
            GroupName = reservation.Group?.Name,
            IsRecurring = reservation.IsRecurring,
            StartDateTime = reservation.StartDateTime,
            EndDateTime = reservation.EndDateTime,
            UserId = reservation.UserId,
            UserName = GetUserFullName(reservation.User)
        };
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            RoleName = user.RoleName,
            RoleDescription = user.Role?.Description ?? ""
        };
    }

    private static string GetUserFullName(User? user)
    {
        if (user == null)
            return "Unknown";

        return $"{user.FirstName} {user.LastName}".Trim();
    }
}
