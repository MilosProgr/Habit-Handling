using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Features.Tags.DTO;

namespace DevHabit.Api.Features.Tags.Queries;

public sealed record TagsCollectionDto : ICollectionResponse<TagDto>
{
    public List<TagDto> Items { get; init; }
}
