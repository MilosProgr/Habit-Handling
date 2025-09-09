using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.DTOs.Entries.ImportJob;

public sealed record EntryImportJobParameters : AcceptHeaderDto
{
    public string? Fields { get; init; }
}
