using DPBloom.Application.Topic.Contracts;
using FluentValidation;

namespace DPBloom.Application.Topic.Validators;

public class CreateTopicValidator : AbstractValidator<CreateTopic>
{
    public CreateTopicValidator()
    {
        RuleFor(ct => ct.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.");
        
        RuleFor(ct => ct.Description)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleFor(ct => ct.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(ct => ct.AuthorId)
            .NotEmpty().WithMessage("AuthorId is required");
    }
}