using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.Entries;

public sealed record EntryParameters : AcceptHeaderDto
{
    public string? Fields { get; init; }
}
