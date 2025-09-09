﻿using FluentValidation;

namespace DevHabit.Api.DTOs.Auth;

public sealed class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name must not be empty")
            .MaximumLength(100)
            .WithMessage("Name can not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email must not be empty")
            .MaximumLength(300)
            .WithMessage("Email can not exceed 300 characters")
            .EmailAddress()
            .WithMessage("Must be a valid email address");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password must not be empty");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirmation password must not be empty")
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");
    }
}
