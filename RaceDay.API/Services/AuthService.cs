using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs.Auth;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class AuthService : IAuthService
{
    private readonly RaceDayDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(RaceDayDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return null;

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role,
        Token = _tokenService.GenerateToken(user),
        ExpiresAt = _tokenService.GetExpiry()
    };
}