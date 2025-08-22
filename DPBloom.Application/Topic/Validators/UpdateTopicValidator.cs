using DPBloom.Application.Topic.Contracts;
using FluentValidation;

namespace DPBloom.Application.Topic.Validators;

public class UpdateTopicValidator : AbstractValidator<UpdateTopic>
{
    public UpdateTopicValidator()
    {
        RuleFor(ut => ut).NotNull().WithMessage("Topic object cannot be null.");

        RuleFor(ut => ut.Title)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Title))
            .WithMessage("Title must be at least 3 characters long.");

        RuleFor(ut => ut.Description)
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Description))
            .WithMessage("Description must be at least 3 characters long.");
    }
}