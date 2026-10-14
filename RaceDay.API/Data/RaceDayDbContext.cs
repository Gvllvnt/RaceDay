using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // One participant per category
        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantId, e.CategoryId })
            .IsUnique();

        // One result per enrolment
        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentId)
            .IsUnique();

        // ---- FIX: Prevent multiple cascade paths ----

        // Event -> Organiser: NoAction (avoid cascade from Users)
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrolment -> Participant: NoAction
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrolment -> Category: Cascade (only this one)
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category -> Event: NoAction
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Result -> Enrolment: Cascade (only this one)
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // BCrypt hash of "Password123!"
        var hash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, Email = "john@raceday.co.za", PasswordHash = hash, FullName = "John Van Der Merwe", Role = "Organiser", CreatedAt = new DateTime(2026, 1, 1) },
            new User { UserId = 2, Email = "sarah@raceday.co.za", PasswordHash = hash, FullName = "Sarah Naidoo", Role = "Organiser", CreatedAt = new DateTime(2026, 1, 2) },
            new User { UserId = 3, Email = "thabo@gmail.com", PasswordHash = hash, FullName = "Thabo Mokoena", Role = "Participant", CreatedAt = new DateTime(2026, 1, 3) },
            new User { UserId = 4, Email = "lisa@gmail.com", PasswordHash = hash, FullName = "Lisa Van Wyk", Role = "Participant", CreatedAt = new DateTime(2026, 1, 4) }
        );

        modelBuilder.Entity<Event>().HasData(
            new Event { EventId = 1, OrganiserId = 1, Title = "Comrades Marathon 2026", Description = "The Ultimate Human Race - 90km", EventDate = new DateTime(2026, 6, 1), Location = "Pietermaritzburg to Durban", Distance = 90, EventType = "run", IsActive = true },
            new Event { EventId = 2, OrganiserId = 1, Title = "Cape Town Cycle Tour 2026", Description = "World's largest timed cycle race", EventDate = new DateTime(2026, 3, 8), Location = "Cape Town", Distance = 109, EventType = "cycle", IsActive = true },
            new Event { EventId = 3, OrganiserId = 2, Title = "Two Oceans Marathon 2026", Description = "World's most beautiful marathon", EventDate = new DateTime(2026, 4, 15), Location = "Cape Town", Distance = 56, EventType = "run", IsActive = true }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, EventId = 1, Name = "Open Men (Ultra)", DistanceKm = 90, StartTime = new TimeOnly(6, 0), EntryFee = 800, MaxParticipants = 5000 },
            new Category { CategoryId = 2, EventId = 1, Name = "Open Women (Ultra)", DistanceKm = 90, StartTime = new TimeOnly(6, 0), EntryFee = 800, MaxParticipants = 4500 },
            new Category { CategoryId = 3, EventId = 2, Name = "Open Men (Cycle)", DistanceKm = 109, StartTime = new TimeOnly(6, 30), EntryFee = 700, MaxParticipants = 10000 },
            new Category { CategoryId = 4, EventId = 3, Name = "Ultra Marathon Men", DistanceKm = 56, StartTime = new TimeOnly(5, 45), EntryFee = 950, MaxParticipants = 3000 }
        );
    }
}
