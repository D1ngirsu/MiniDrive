using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Identity.Entities;

namespace MiniDrive.Identity.Repositories;

public class UserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existing = await _context.Users.FindAsync(user.Id);
        if (existing == null)
        {
            return false;
        }

        _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SessionInfo> CreateSessionAsync(
        Guid userId,
        TimeSpan lifetime,
        string? userAgent,
        string? ipAddress)
    {
        await CleanupExpiredSessionsAsync();

        var tokenBytes = RandomNumberGenerator.GetBytes(24);
        var token = Convert.ToHexString(tokenBytes);

        var session = new Session
        {
            Token = token,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime),
            UserAgent = userAgent,
            IpAddress = ipAddress
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        return SessionInfo.FromSession(session);
    }

    public async Task<SessionInfo?> GetSessionAsync(string token)
    {
        var session = await _context.Sessions.FindAsync(token);
        if (session == null)
        {
            return null;
        }

        if (session.IsExpired)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return null;
        }

        return SessionInfo.FromSession(session);
    }

    public async Task<bool> RemoveSessionAsync(string token)
    {
        var session = await _context.Sessions.FindAsync(token);
        if (session == null)
        {
            return false;
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> RemoveSessionsForUserAsync(Guid userId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _context.Sessions.RemoveRange(sessions);
        await _context.SaveChangesAsync();
        return sessions.Count;
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        var expiredSessions = await _context.Sessions
            .Where(s => s.ExpiresAtUtc <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredSessions.Any())
        {
            _context.Sessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
        }
    }
}
