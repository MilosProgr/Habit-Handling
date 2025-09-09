using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevHabit.Api.DTOs.Entries.ImportJob;

[ValidateNever]
public sealed record CreateEntryImportJobDto
{
    public required IFormFile File { get; init; }
}
