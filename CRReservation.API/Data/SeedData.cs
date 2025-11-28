using CRReservation.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists - safely check for tables
        try
        {
            if (await context.Roles.AnyAsync() || await context.ClassRooms.AnyAsync())
            {
                return; // DB has been seeded
            }
        }
        catch
        {
            // Tables don't exist yet, continue with seeding
        }

        // Seed Roles
        var roles = new[]
        {
            new Role { Name = "admin", Description = "Administrator systemu" },
            new Role { Name = "prowadzacy", Description = "Prowadzący zajęcia" },
            new Role { Name = "student", Description = "Student" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // Seed ClassRooms (Sale)
        var classRooms = new[]
        {
            new ClassRoom { Name = "Sala 101", Capacity = 30, IsActive = true },
            new ClassRoom { Name = "Sala 202", Capacity = 50, IsActive = true },
            new ClassRoom { Name = "Sala 303", Capacity = 20, IsActive = true }
        };

        await context.ClassRooms.AddRangeAsync(classRooms);
        await context.SaveChangesAsync();

        // Seed Users
        var users = new[]
        {
            new User
            {
                FirstName = "Jan",
                LastName = "Kowalski",
                RoleName = "admin",
                Email = "jan.kowalski@example.com"
            },
            new User
            {
                FirstName = "Anna",
                LastName = "Nowak",
                RoleName = "prowadzacy",
                Email = "anna.nowak@example.com"
            },
            new User
            {
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                RoleName = "student",
                Email = "piotr.wisniewski@example.com"
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Seed Groups
        var groups = new[]
        {
            new Group
            {
                Name = "Informatyka I rok",
                Description = "Studenci pierwszego roku informatyki"
            },
            new Group
            {
                Name = "Zarządzanie II rok",
                Description = "Studenci drugiego roku zarządzania"
            }
        };

        await context.Groups.AddRangeAsync(groups);
        await context.SaveChangesAsync();

        // Seed UserGroups
        var userGroups = new[]
        {
            new UserGroup { GroupId = 1, UserId = 3 }, // Piotr in Informatyka I rok
            new UserGroup { GroupId = 2, UserId = 3 }  // Piotr in Zarządzanie II rok
        };

        await context.UserGroups.AddRangeAsync(userGroups);
        await context.SaveChangesAsync();

        // Seed Reservations
        var reservations = new[]
        {
            new Reservation
            {
                Status = "potwierdzona",
                ClassRoomId = 1,
                ReservationDate = DateTime.Parse("2025-12-01"),
                GroupId = 1,
                IsRecurring = false,
                StartDateTime = DateTime.Parse("2025-12-01 10:00:00"),
                EndDateTime = DateTime.Parse("2025-12-01 12:00:00"),
                UserId = 2 // Anna Nowak
            },
            new Reservation
            {
                Status = "oczekujaca",
                ClassRoomId = 2,
                ReservationDate = DateTime.Parse("2025-12-02"),
                GroupId = 2,
                IsRecurring = true,
                StartDateTime = DateTime.Parse("2025-12-02 14:00:00"),
                EndDateTime = DateTime.Parse("2025-12-02 16:00:00"),
                UserId = 2 // Anna Nowak
            }
        };

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}
