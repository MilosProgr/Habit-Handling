using System.Linq.Expressions;
using DevHabit.Api.Features.Tags.DTO;
using DevHabit.Api.Features.Tags.Entities;

namespace DevHabit.Api.Features.Tags.Queries;

internal static class TagQueries
{
    public static Expression<Func<Tag, TagDto>> ProjectToDto()
    {
        return t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            CreatedAtUtc = t.CreatedAtUtc,
            UpdatedAtUtc = t.UpdatedAtUtc
        };
    }
}
