using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevHabit.Api.DTOs.Users;

[ValidateNever]
public sealed class UpdateProfileDto
{
    public required string Name { get; set; }
}
