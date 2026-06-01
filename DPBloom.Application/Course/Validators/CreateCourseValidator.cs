using DPBloom.Application.Course.Contracts;
using FluentValidation;

namespace DPBloom.Application.Course.Validators;

public class CreateCourseValidator : AbstractValidator<CreateCourseDto>
{
    public CreateCourseValidator()
    {
        RuleFor(cc => cc).NotNull().WithMessage("Course object cannot be null.");
        
        RuleFor(cc => cc.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.");
        
        //TODO: check, if it working properly
        RuleFor(cc => cc.Description)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
    }
}