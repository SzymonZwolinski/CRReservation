using CRReservation.API.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CRReservation.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if roles already exist
        try
        {
            if (await context.Roles.AnyAsync())
            {
                return; // DB has been initialized with roles
            }
        }
        catch
        {
            // Tables don't exist yet, continue with seeding
        }

        // Seed Roles - only essential roles
        var roles = new[]
        {
            new Role { Name = "admin", Description = "Administrator systemu" },
            new Role { Name = "prowadzacy", Description = "Prowadzący zajęcia" },
            new Role { Name = "student", Description = "Student" }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
