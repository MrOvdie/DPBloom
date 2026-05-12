using DPBloom.Application.Topic.Contracts;
using FluentValidation;

namespace DPBloom.Application.Topic.Validators;

public class UpdateTopicValidator : AbstractValidator<UpdateTopic>
{
    public UpdateTopicValidator()
    {
        RuleFor(ut => ut).NotNull().WithMessage("Topic object cannot be null.");

        RuleFor(ut => ut.Title)
            .NotEmpty().When(x => x.Title is not null)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Title))
            .WithMessage("Title must be at least 3 characters long.");

        RuleFor(ut => ut.Description)
            .NotEmpty().When(x => x.Description is not null)
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Description))
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleFor(ut => ut.CourseId)
            .NotEmpty().When(x => x.CourseId != null)
            .WithMessage("CourseId can't be empty.");
    }
}