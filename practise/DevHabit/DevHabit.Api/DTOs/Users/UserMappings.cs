using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = $"u_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Email = dto.Email,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void UpdateFromDto(this User user, UpdateProfileDto dto)
    {
        user.Name = dto.Name;
        user.UpdatedAtUtc = DateTime.UtcNow;
    }
}
