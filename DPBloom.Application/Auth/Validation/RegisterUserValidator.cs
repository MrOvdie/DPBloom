using DPBloom.Application.Auth.Contracts;
using FluentValidation;

namespace DPBloom.Application.Auth.Validation;

public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Incorrect Email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Пароль має містити щонайменше 6 символів.")
            //.Matches(@"[A-Z]+").WithMessage("Пароль має містити хоча б одну велику літеру.")
            .Matches(@"[a-z]+").WithMessage("Пароль має містити хоча б одну малу літеру.")
            .Matches(@"[0-9]+").WithMessage("Пароль має містити хоча б одну цифру.");
            // .Matches(@"[\!\?\*\.]+").WithMessage("Пароль має містити спецсимвол."); // Якщо колись знадобиться
    }
}