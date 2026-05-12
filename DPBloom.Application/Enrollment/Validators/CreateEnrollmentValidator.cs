using DPBloom.Application.Enrollment.Contracts;
using FluentValidation;

namespace DPBloom.Application.Enrollment.Validators;

public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollment>
{
    public CreateEnrollmentValidator()
    {
        RuleFor(ce => ce).NotNull().WithMessage("Course object cannot be null.");

        /*RuleFor(ce => ce.Status)
            .InclusiveBetween(0, 3).WithMessage("Excided grade range.");*/ //TODO: make proper validation

        RuleFor(ce => ce.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(ce => ce.CourseId)
            .NotEmpty().WithMessage("Course id is required.");
    }
}