using System.Linq.Expressions;
using DevHabit.Api.Features.Users.DTO;
using DevHabit.Api.Features.Users.Entities;

namespace DevHabit.Api.Features.Users.Queries;

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
