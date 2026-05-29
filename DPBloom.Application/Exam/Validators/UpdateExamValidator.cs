using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Update;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class UpdateExamValidator : AbstractValidator<UpdateExamDto>
{
    public UpdateExamValidator()
    {
        RuleFor(ce => ce).NotNull().WithMessage("Exam object cannot be null.");
        
        RuleFor(ce => ce.Title)
            .NotEmpty().When(x => x.Title is not null)
            .WithMessage("Title can't be empty.")
            .MaximumLength(150)
            .WithMessage("Title cannot be longer than 150 characters.");
        
        RuleFor(ce => ce.Description)
            .NotEmpty().When(x => x.Description is not null)
            .MaximumLength(1000).WithMessage("Description cannot be longer than 1000 characters.")
            .When(ce => !string.IsNullOrEmpty(ce.Description));

        RuleFor(ce => ce.TopicId)
            .NotEmpty().When(x => x.TopicId != null)
            .WithMessage("TopicId can't be empty.");
        
        RuleFor(ce => ce.Duration)
            .NotEmpty().When(x => x.Duration != null)
            .Must(d => d > TimeSpan.Zero).WithMessage("Duration must be greater than zero.")
            .Must(d => d.Value.TotalHours <= 8).WithMessage("Duration cannot exceed 8 hours.");

        RuleFor(ce => ce.FinishesAt)
            .NotEmpty().When(x => x.FinishesAt != null)
            .GreaterThan(ce => ce.StartsAt).WithMessage("Exam finish time must be after start time.");
        
        RuleFor(ce => ce.MinimalPassScore)
            .NotEmpty().When(x => x.MinimalPassScore is not null)
            .InclusiveBetween(0, 100).WithMessage("Minimal pass score must be between 0 and 100.");
        
        RuleFor(ce => ce.AttemptsCount)
            .InclusiveBetween(1, 100).WithMessage("Attempts count must be between 1 and 100.");

        RuleForEach(ce => ce.Questions)
            .SetValidator(new UpdateQuestionValidator());
    }
}