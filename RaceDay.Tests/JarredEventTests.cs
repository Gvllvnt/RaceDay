using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

/// <summary>
/// Jarred's Event Tests
/// Jarred loves managing events, so he tests them all!
/// </summary>
public class JarredEventTests
{
    private RaceDayDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task Jarred_Can_Create_An_Event()
    {
        // Arrange - Jarred the Organiser wants to create a race
        var context = GetInMemoryContext();
        var jarred = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred Organiser", PasswordHash = "x", Role = "Organiser" };
        context.Users.Add(jarred);
        await context.SaveChangesAsync();

        var newEvent = new Event
        {
            OrganiserId = 1,
            Title = "Jarred's Super Marathon",
            Description = "The best marathon Jarred ever organised",
            EventDate = new DateTime(2026, 8, 1),
            Location = "Cape Town",
            Distance = 42.2m,
            EventType = "run",
            IsActive = true
        };

        // Act
        context.Events.Add(newEvent);
        await context.SaveChangesAsync();

        // Assert
        var saved = await context.Events.FirstOrDefaultAsync(e => e.Title == "Jarred's Super Marathon");
        saved.Should().NotBeNull();
        saved!.Location.Should().Be("Cape Town");
        saved.Distance.Should().Be(42.2m);
        saved.EventType.Should().Be("run");
    }

    [Fact]
    public async Task Jarred_Can_Update_His_Event()
    {
        // Arrange - Jarred wants to fix a typo in his event
        var context = GetInMemoryContext();
        var jarred = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred", PasswordHash = "x", Role = "Organiser" };
        context.Users.Add(jarred);

        var evt = new Event
        {
            EventId = 1,
            OrganiserId = 1,
            Title = "Old Title",
            EventDate = new DateTime(2026, 8, 1),
            Location = "Durban",
            IsActive = true
        };
        context.Events.Add(evt);
        await context.SaveChangesAsync();

        // Act - Jarred updates it
        var toUpdate = await context.Events.FindAsync(1);
        toUpdate!.Title = "Jarred's Updated Marathon";
        toUpdate.Location = "Cape Town";
        await context.SaveChangesAsync();

        // Assert
        var updated = await context.Events.FindAsync(1);
        updated!.Title.Should().Be("Jarred's Updated Marathon");
        updated.Location.Should().Be("Cape Town");
    }

    [Fact]
    public async Task Jarred_Can_Soft_Delete_His_Event()
    {
        // Arrange - Jarred wants to hide an event
        var context = GetInMemoryContext();
        var jarred = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred", PasswordHash = "x", Role = "Organiser" };
        context.Users.Add(jarred);

        var evt = new Event
        {
            EventId = 1,
            OrganiserId = 1,
            Title = "To Be Deleted",
            EventDate = new DateTime(2026, 8, 1),
            Location = "Cape Town",
            IsActive = true
        };
        context.Events.Add(evt);
        await context.SaveChangesAsync();

        // Act
        var toDelete = await context.Events.FindAsync(1);
        toDelete!.IsActive = false;
        await context.SaveChangesAsync();

        // Assert
        var deleted = await context.Events.FindAsync(1);
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Jarred_Sees_Only_Active_Events()
    {
        // Arrange - Jarred has both active and inactive events
        var context = GetInMemoryContext();
        var jarred = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred", PasswordHash = "x", Role = "Organiser" };
        context.Users.Add(jarred);

        context.Events.AddRange(
            new Event { EventId = 1, OrganiserId = 1, Title = "Active Event", EventDate = DateTime.Now.AddDays(10), Location = "A", IsActive = true },
            new Event { EventId = 2, OrganiserId = 1, Title = "Inactive Event", EventDate = DateTime.Now.AddDays(10), Location = "B", IsActive = false }
        );
        await context.SaveChangesAsync();

        // Act
        var activeEvents = await context.Events.Where(e => e.IsActive).ToListAsync();

        // Assert
        activeEvents.Should().HaveCount(1);
        activeEvents[0].Title.Should().Be("Active Event");
    }

    [Fact]
    public async Task Jarred_Can_Add_Categories_To_His_Event()
    {
        // Arrange - Jarred wants categories for his event
        var context = GetInMemoryContext();
        var jarred = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred", PasswordHash = "x", Role = "Organiser" };
        context.Users.Add(jarred);

        var evt = new Event
        {
            EventId = 1,
            OrganiserId = 1,
            Title = "Jarred's Event",
            EventDate = new DateTime(2026, 8, 1),
            Location = "Cape Town",
            IsActive = true
        };
        context.Events.Add(evt);
        await context.SaveChangesAsync();

        // Act
        context.Categories.AddRange(
            new Category { CategoryId = 1, EventId = 1, Name = "10km", DistanceKm = 10, StartTime = new TimeOnly(7, 0), EntryFee = 100, MaxParticipants = 500 },
            new Category { CategoryId = 2, EventId = 1, Name = "21km", DistanceKm = 21, StartTime = new TimeOnly(6, 30), EntryFee = 200, MaxParticipants = 300 }
        );
        await context.SaveChangesAsync();

        // Assert
        var categories = await context.Categories.Where(c => c.EventId == 1).ToListAsync();
        categories.Should().HaveCount(2);
        categories.Should().Contain(c => c.Name == "10km");
        categories.Should().Contain(c => c.Name == "21km");
    }
}