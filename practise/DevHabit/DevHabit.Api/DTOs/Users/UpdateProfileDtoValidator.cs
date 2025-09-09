using FluentValidation;

namespace DevHabit.Api.DTOs.Users;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name must not be empty")
            .MaximumLength(100)
            .WithMessage("Name can not exceed 100 characters");
    }
}

