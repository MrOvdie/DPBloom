using FluentValidation;

namespace DPBloom.Application.Common.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> PasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Пароль має містити щонайменше 6 символів.")
            //.Matches(@"[A-Z]+").WithMessage("Пароль має містити хоча б одну велику літеру.")
            .Matches(@"[a-z]+").WithMessage("Пароль має містити хоча б одну малу літеру.")
            .Matches(@"[0-9]+").WithMessage("Пароль має містити хоча б одну цифру.");
        // .Matches(@"[\!\?\*\.]+").WithMessage("Пароль має містити спецсимвол."); // Якщо колись знадобиться
    }
}