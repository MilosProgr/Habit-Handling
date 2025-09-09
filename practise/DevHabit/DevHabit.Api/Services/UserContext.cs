using System.Security.Claims;
using DevHabit.Api.Database;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.Api.Services;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache
)
{
    private const string CacheKeyPrefix = "users:id:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);


    public async Task<string?> GetUserIdAsync(CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {

            return null;
        }

        // ⚡ Koristi NameIdentifier jer middleware mapira sub claim u njega
        string? identityId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("sub")?.Value;

        var email = user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(identityId))
        {

            return null;
        }

        string cacheKey = $"{CacheKeyPrefix}{identityId}";

        // Keširanje korisnika
        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);

            var existingUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.IdentifyId == identityId, cancellationToken);

            if (existingUser != null)
            {

                return existingUser.Id;
            }
            var fullName = user.FindFirst("name")?.Value ?? "";
            var firstName = fullName.Split(' ')[0];

            // Kreiraj novog korisnika iz tokena
            var newUser = new User
            {
                Id = User.CreateNewId(),
                IdentifyId = identityId,
                Name = firstName ?? "",
                Email = email?? "",
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync(cancellationToken);

            return newUser.Id;
        });
    }
}
