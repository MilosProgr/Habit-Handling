namespace DevHabit.Api.Features.Tags.Operations;

public sealed record UpdateTagDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
