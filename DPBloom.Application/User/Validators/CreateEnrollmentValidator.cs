using DPBloom.Application.User.Contracts;
using FluentValidation;

namespace DPBloom.Application.User.Validators;

public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollment>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(ce => ce).NotNull().WithMessage("Course object cannot be null.");
        
        RuleFor(ce => ce.Status)
            .InclusiveBetween(0, 3).WithMessage("Excided grade range.");
        
        RuleFor(ce => ce.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}