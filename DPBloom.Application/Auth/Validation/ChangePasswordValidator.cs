using DPBloom.Application.Auth.Contracts;
using DPBloom.Application.Common.Validation;
using FluentValidation;

namespace DPBloom.Application.Auth.Validation;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
     
        RuleFor(x => x.NewPassword).PasswordRules();
    }
}