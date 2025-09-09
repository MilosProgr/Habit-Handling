using System.Linq.Expressions;

using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.NewFolder;

internal static class UserQueries
{
    public static Expression<Func<User,UserDto>> ProjectToDto()
    {
        return t => new UserDto
        {
            Id = t.Id,
            Email = t.Email,
            Name = t.Name,
            CreatedAtUtc = t.CreatedAtUtc,
            UpdatedAtUtc = t.UpdatedAtUtc,
            IdentifyId = t.IdentifyId
        };
    }
}
