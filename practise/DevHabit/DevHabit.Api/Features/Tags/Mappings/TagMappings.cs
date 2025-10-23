using DevHabit.Api.Common.Sorting;
using DevHabit.Api.Features.Tags.DTO;
using DevHabit.Api.Features.Tags.Entities;
using DevHabit.Api.Features.Tags.Operations;

namespace DevHabit.Api.Features.Tags.Mappings;

internal static class TagMappings
{
    public static readonly SortMappingDefinition<TagDto, Tag> SortMapping = new()
    {
        Mappings =
        [
            new SortMapping(nameof(TagDto.Name), nameof(Tag.Name)),
            new SortMapping(nameof(TagDto.Description), nameof(Tag.Description)),
            new SortMapping(nameof(TagDto.CreatedAtUtc), nameof(Tag.CreatedAtUtc)),
            new SortMapping(nameof(TagDto.UpdatedAtUtc), nameof(Tag.UpdatedAtUtc))
        ]
    };
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }

    public static Tag ToEntity(this CreateTagDto dto,string userId)
    {
        Tag habit = new()
        {
            Id = $"t_{Guid.CreateVersion7()}",
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        return habit;
    }

    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
