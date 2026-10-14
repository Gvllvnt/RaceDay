using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

/// <summary>
/// Jarred's Authentication Tests
/// Jarred loves unit tests, so Jarred tests everything about auth!
/// </summary>
public class JarredAuthTests
{
    private RaceDayDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    private IAuthService GetAuthService(RaceDayDbContext context)
    {
        var tokenService = new JarredFakeTokenService();
        return new AuthService(context, tokenService);
    }

    [Fact]
    public async Task Jarred_Can_Register_As_Participant()
    {
        // Arrange - Jarred wants to sign up as a Participant
        var context = GetInMemoryContext();
        var service = GetAuthService(context);
        var dto = new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = "JarredLovesTests123!",
            FullName = "Jarred The Tester",
            Role = "Participant"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("jarred@raceday.co.za");
        result.Role.Should().Be("Participant");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Jarred_Can_Register_As_Organiser()
    {
        // Arrange - Jarred decides to be an Organiser too
        var context = GetInMemoryContext();
        var service = GetAuthService(context);
        var dto = new RegisterDto
        {
            Email = "jarred.organiser@raceday.co.za",
            Password = "JarredIsTheBest123!",
            FullName = "Jarred Organiser",
            Role = "Organiser"
        };

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().Be("Organiser");
    }

    [Fact]
    public async Task Jarred_Cannot_Register_With_Duplicate_Email()
    {
        // Arrange - Jarred tries to register twice
        var context = GetInMemoryContext();
        var service = GetAuthService(context);
        var dto = new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = "Password123!",
            FullName = "Jarred First",
            Role = "Participant"
        };
        await service.RegisterAsync(dto);

        // Act - try again
        var duplicateDto = new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = "DifferentPassword!",
            FullName = "Jarred Second",
            Role = "Participant"
        };
        var result = await service.RegisterAsync(duplicateDto);

        // Assert
        result.Should().BeNull(); // Registration should fail
    }

    [Fact]
    public async Task Jarred_Can_Login_With_Correct_Credentials()
    {
        // Arrange - Jarred registers and then logs in
        var context = GetInMemoryContext();
        var service = GetAuthService(context);

        await service.RegisterAsync(new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = "JarredPassword123!",
            FullName = "Jarred",
            Role = "Participant"
        });

        // Act
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "jarred@raceday.co.za",
            Password = "JarredPassword123!"
        });

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("jarred@raceday.co.za");
    }

    [Fact]
    public async Task Jarred_Cannot_Login_With_Wrong_Password()
    {
        // Arrange - Jarred registers but forgets his password
        var context = GetInMemoryContext();
        var service = GetAuthService(context);

        await service.RegisterAsync(new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = "JarredPassword123!",
            FullName = "Jarred",
            Role = "Participant"
        });

        // Act - tries wrong password
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "jarred@raceday.co.za",
            Password = "WrongPassword!"
        });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Jarred_Cannot_Login_With_Unknown_Email()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = GetAuthService(context);

        // Act - Jarred tries email that doesn't exist
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "unknown@raceday.co.za",
            Password = "Password123!"
        });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Jarreds_Password_Is_Hashed_Not_Plain_Text()
    {
        // Arrange - Jarred wants his password safe
        var context = GetInMemoryContext();
        var service = GetAuthService(context);
        var plainPassword = "JarredSecret123!";

        await service.RegisterAsync(new RegisterDto
        {
            Email = "jarred@raceday.co.za",
            Password = plainPassword,
            FullName = "Jarred",
            Role = "Participant"
        });

        // Act
        var user = await context.Users.FirstAsync(u => u.Email == "jarred@raceday.co.za");

        // Assert
        user.PasswordHash.Should().NotBe(plainPassword);
        user.PasswordHash.Should().NotBeNullOrEmpty();
        user.PasswordHash.Length.Should().BeGreaterThan(20);
    }
}

/// <summary>
/// Fake token service for testing - Jarred doesn't need real JWTs to test
/// </summary>
public class JarredFakeTokenService : ITokenService
{
    public string GenerateToken(User user) => $"JARRED_FAKE_TOKEN_{user.UserId}";
    public DateTime GetExpiry() => DateTime.UtcNow.AddHours(8);
}
