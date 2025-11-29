using CRReservation.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRReservation.API.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<User> Users { get; set; }
    public DbSet<ClassRoom> ClassRooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Name);
            entity.Property(r => r.Name).HasMaxLength(20);
            entity.Property(r => r.Description).HasMaxLength(100);
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.RoleName).HasMaxLength(20).IsRequired();

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleName)
                  .HasPrincipalKey(r => r.Name)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ClassRoom entity
        modelBuilder.Entity<ClassRoom>(entity =>
        {
            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.Name).HasMaxLength(100).IsRequired();
            entity.Property(cr => cr.Capacity).IsRequired();
            entity.Property(cr => cr.IsActive).IsRequired();
            entity.Property(cr => cr.Notes).HasMaxLength(500);
        });

        // Configure Group entity
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).HasMaxLength(100).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(500);
        });

        // Configure UserGroup many-to-many relationship
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(ug => ug.Id);
            entity.HasIndex(ug => new { ug.GroupId, ug.UserId }).IsUnique();

            entity.HasOne(ug => ug.Group)
                  .WithMany(g => g.UserGroups)
                  .HasForeignKey(ug => ug.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ug => ug.User)
                  .WithMany(u => u.UserGroups)
                  .HasForeignKey(ug => ug.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Reservation entity
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasMaxLength(20).IsRequired();
            entity.Property(r => r.ReservationDate).IsRequired();
            entity.Property(r => r.IsRecurring).IsRequired();
            entity.Property(r => r.StartDateTime).IsRequired();
            entity.Property(r => r.EndDateTime).IsRequired();

            entity.HasOne(r => r.ClassRoom)
                  .WithMany(cr => cr.Reservations)
                  .HasForeignKey(r => r.ClassRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Group)
                  .WithMany(g => g.Reservations)
                  .HasForeignKey(r => r.GroupId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}