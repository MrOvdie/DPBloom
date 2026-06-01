using DPBloom.Application.Topic.Contracts;
using FluentValidation;

namespace DPBloom.Application.Topic.Validators;

public class CreateTopicValidator : AbstractValidator<CreateTopicDto>
{
    public CreateTopicValidator()
    {
        RuleFor(ct => ct).NotNull().WithMessage("Topic object cannot be null.");
        
        RuleFor(ct => ct.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.");
        
        RuleFor(ct => ct.Description)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
    }
}