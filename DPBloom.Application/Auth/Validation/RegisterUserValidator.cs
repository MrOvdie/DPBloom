using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.Common.Validation;
using FluentValidation;

namespace DPBloom.Application.Auth.Validation;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Incorrect Email format.");

        RuleFor(x => x.Password).PasswordRules();

    }
}