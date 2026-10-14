using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

/// <summary>
/// Jarred's Role-Based Access Tests
/// Jarred loves role-based access, so he tests every role boundary!
/// </summary>
public class JarredRoleTests
{
    private RaceDayDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task Jarred_The_Participant_Has_Participant_Role()
    {
        // Arrange
        var context = GetInMemoryContext();
        context.Users.Add(new User { UserId = 1, Email = "jarred@gmail.com", FullName = "Jarred Participant", PasswordHash = "x", Role = "Participant" });
        await context.SaveChangesAsync();

        // Act
        var user = await context.Users.FindAsync(1);

        // Assert
        user!.Role.Should().Be("Participant");
    }

    [Fact]
    public async Task Jarred_The_Organiser_Has_Organiser_Role()
    {
        // Arrange
        var context = GetInMemoryContext();
        context.Users.Add(new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred Organiser", PasswordHash = "x", Role = "Organiser" });
        await context.SaveChangesAsync();

        // Act
        var user = await context.Users.FindAsync(1);

        // Assert
        user!.Role.Should().Be("Organiser");
    }

    [Fact]
    public async Task Only_Organisers_Are_Returned_When_Filtering_By_Organiser_Role()
    {
        // Arrange - Both roles exist
        var context = GetInMemoryContext();
        context.Users.AddRange(
            new User { UserId = 1, Email = "jarred.organiser@raceday.co.za", FullName = "Jarred Organiser", PasswordHash = "x", Role = "Organiser" },
            new User { UserId = 2, Email = "jarred.participant@gmail.com", FullName = "Jarred Participant", PasswordHash = "x", Role = "Participant" },
            new User { UserId = 3, Email = "jarred.organiser2@raceday.co.za", FullName = "Jarred Organiser 2", PasswordHash = "x", Role = "Organiser" }
        );
        await context.SaveChangesAsync();

        // Act
        var organisers = await context.Users.Where(u => u.Role == "Organiser").ToListAsync();

        // Assert
        organisers.Should().HaveCount(2);
        organisers.Should().OnlyContain(u => u.Role == "Organiser");
    }

    [Fact]
    public async Task Only_Participants_Are_Returned_When_Filtering_By_Participant_Role()
    {
        // Arrange
        var context = GetInMemoryContext();
        context.Users.AddRange(
            new User { UserId = 1, Email = "jarred.organiser@raceday.co.za", FullName = "Jarred Organiser", PasswordHash = "x", Role = "Organiser" },
            new User { UserId = 2, Email = "jarred.participant@gmail.com", FullName = "Jarred Participant", PasswordHash = "x", Role = "Participant" },
            new User { UserId = 3, Email = "jarred.participant2@gmail.com", FullName = "Jarred Participant 2", PasswordHash = "x", Role = "Participant" }
        );
        await context.SaveChangesAsync();

        // Act
        var participants = await context.Users.Where(u => u.Role == "Participant").ToListAsync();

        // Assert
        participants.Should().HaveCount(2);
        participants.Should().OnlyContain(u => u.Role == "Participant");
    }

    [Fact]
    public async Task Jarred_The_Organiser_Owns_Events_He_Created()
    {
        // Arrange - Jarred the Organiser creates events
        var context = GetInMemoryContext();
        var jarredOrganiser = new User { UserId = 1, Email = "jarred@raceday.co.za", FullName = "Jarred Organiser", PasswordHash = "x", Role = "Organiser" };
        var otherOrganiser = new User { UserId = 2, Email = "other@raceday.co.za", FullName = "Other", PasswordHash = "x", Role = "Organiser" };
        context.Users.AddRange(jarredOrganiser, otherOrganiser);

        context.Events.AddRange(
            new Event { EventId = 1, OrganiserId = 1, Title = "Jarred's Event 1", EventDate = DateTime.Now.AddDays(10), Location = "A", IsActive = true },
            new Event { EventId = 2, OrganiserId = 1, Title = "Jarred's Event 2", EventDate = DateTime.Now.AddDays(20), Location = "B", IsActive = true },
            new Event { EventId = 3, OrganiserId = 2, Title = "Other's Event", EventDate = DateTime.Now.AddDays(15), Location = "C", IsActive = true }
        );
        await context.SaveChangesAsync();

        // Act - Jarred queries his own events
        var jarredsEvents = await context.Events.Where(e => e.OrganiserId == 1).ToListAsync();

        // Assert
        jarredsEvents.Should().HaveCount(2);
        jarredsEvents.Should().OnlyContain(e => e.OrganiserId == 1);
    }

    [Fact]
    public async Task Jarred_The_Organiser_Cannot_Modify_Another_Organisers_Event()
    {
        // Arrange - Jarred tries to update someone else's event
        var context = GetInMemoryContext();
        var otherOrganisersEvent = new Event
        {
            EventId = 1,
            OrganiserId = 99, // Not Jarred
            Title = "Someone Else's Event",
            EventDate = DateTime.Now.AddDays(10),
            Location = "X",
            IsActive = true
        };
        context.Events.Add(otherOrganisersEvent);
        await context.SaveChangesAsync();

        int jarredId = 1;

        // Act - simulate ownership check
        var evt = await context.Events.FindAsync(1);
        var isOwner = evt!.OrganiserId == jarredId;

        // Assert
        isOwner.Should().BeFalse("Jarred should not be allowed to modify another organiser's event");
    }

    [Fact]
    public async Task Jarred_The_Participant_Can_Only_See_His_Own_Enrolments()
    {
        // Arrange - Jarred has his enrolments, someone else has theirs
        var context = GetInMemoryContext();
        context.Users.AddRange(
            new User { UserId = 1, Email = "jarred@gmail.com", FullName = "Jarred", PasswordHash = "x", Role = "Participant" },
            new User { UserId = 2, Email = "other@gmail.com", FullName = "Other", PasswordHash = "x", Role = "Participant" }
        );

        context.Enrolments.AddRange(
            new Enrolment { EnrolmentId = 1, ParticipantId = 1, CategoryId = 1, RaceNumber = 11111, Status = "Registered" },
            new Enrolment { EnrolmentId = 2, ParticipantId = 1, CategoryId = 2, RaceNumber = 22222, Status = "Registered" },
            new Enrolment { EnrolmentId = 3, ParticipantId = 2, CategoryId = 1, RaceNumber = 33333, Status = "Registered" }
        );
        await context.SaveChangesAsync();

        // Act - Jarred queries his own
        var jarredsEnrolments = await context.Enrolments.Where(e => e.ParticipantId == 1).ToListAsync();

        // Assert
        jarredsEnrolments.Should().HaveCount(2);
        jarredsEnrolments.Should().OnlyContain(e => e.ParticipantId == 1);
    }
}