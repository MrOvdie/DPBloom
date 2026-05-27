using FluentValidation;

namespace DPBloom.Application.Common.Validation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> PasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            //.Matches(@"[A-Z]+").WithMessage("Пароль має містити хоча б одну велику літеру.")
            .Matches(@"[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]+").WithMessage("Password must contain at least one number.");
        // .Matches(@"[\!\?\*\.]+").WithMessage("Пароль має містити спецсимвол."); // Якщо колись знадобиться
    }
}