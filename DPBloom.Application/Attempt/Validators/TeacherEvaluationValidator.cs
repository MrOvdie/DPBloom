using DPBloom.Application.Exam.Contracts;
using FluentValidation;

namespace DPBloom.Application.Attempt.Validators;

public class TeacherEvaluationValidator : AbstractValidator<TeacherEvaluationDto>
{
    public TeacherEvaluationValidator()
    {
        RuleFor(te => te).NotNull().WithMessage("Exam object cannot be null.");
        
        RuleFor(te => te.QuestionId)
            .NotEmpty().WithMessage("Question id is required.");
        
        RuleFor(te => te.Comment)
            .NotEmpty().When(x => x.Comment is not null)
            .MaximumLength(500).WithMessage("Comment cannot be longer than 1000 characters.");
        
        RuleFor(te => te.AwardedScore)
            .InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100.");
    }
}