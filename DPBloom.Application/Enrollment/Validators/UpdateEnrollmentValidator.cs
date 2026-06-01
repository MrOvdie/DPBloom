using DPBloom.Application.Enrollment.Contracts;
using FluentValidation;

namespace DPBloom.Application.Enrollment.Validators;

public class UpdateEnrollmentValidator : AbstractValidator<UpdateEnrollmentDto>
{
    public UpdateEnrollmentValidator()
    {
        RuleFor(ue => ue).NotNull().WithMessage("Topic object cannot be null.");

        RuleFor(ue => ue.FinalGrade)
            .InclusiveBetween(0.0, 100.0)
            .WithMessage("Grade must be between 0 and 200.")
            .When(x => x.FinalGrade.HasValue);

        /*RuleFor(ue => ue.Status)
            .InclusiveBetween(0, 3).WithMessage("Excided grade range.");*/ //TODO: make proper validation
    }
}