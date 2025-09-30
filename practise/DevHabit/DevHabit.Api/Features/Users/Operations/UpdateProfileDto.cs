using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevHabit.Api.Features.Users.Operations;

[ValidateNever]
public sealed class UpdateProfileDto
{
    public required string Name { get; set; }
}
