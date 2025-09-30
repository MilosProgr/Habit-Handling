using System.ComponentModel.DataAnnotations;

namespace DevHabit.Api.Features.Tags.Operations;

public sealed record CreateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
