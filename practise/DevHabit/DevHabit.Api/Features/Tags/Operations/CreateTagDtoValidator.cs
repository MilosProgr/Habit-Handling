using FluentValidation;

namespace DevHabit.Api.Features.Tags.Operations;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);

        RuleFor(x => x.Description).MaximumLength(50);
    }
}
