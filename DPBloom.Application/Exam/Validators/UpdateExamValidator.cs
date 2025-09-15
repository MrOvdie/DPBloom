using FluentValidation;
using TestOfTesting.DTOs;

namespace DPBloom.Application.Exam.Validators;

public class UpdateExamValidator : AbstractValidator<CreateExamDto>
{
    public UpdateExamValidator()
    {
        RuleFor(ce => ce).NotNull().WithMessage("Exam object cannot be null.");
        
        RuleFor(ce => ce.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(150)
            .WithMessage("Title cannot be longer than 150 characters.");
        
        RuleFor(ce => ce.Description)
            .MaximumLength(1000).WithMessage("Description cannot be longer than 1000 characters.")
            .When(ce => !string.IsNullOrEmpty(ce.Description));

        RuleFor(ce => ce.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(ce => ce.AuthorId)
            .NotEmpty().WithMessage("AuthorId is required.");

        RuleFor(ce => ce.Duration)
            .Must(d => d > TimeSpan.Zero).WithMessage("Duration must be greater than zero.")
            .Must(d => d.TotalHours <= 8).WithMessage("Duration cannot exceed 8 hours.");

        RuleFor(ce => ce.FinishesAt)
            .GreaterThan(ce => ce.StartsAt).WithMessage("Exam finish time must be after start time.");

        RuleForEach(ce => ce.Questions)
            .SetValidator(new QuestionValidator());
    }
}