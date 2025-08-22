using DPBloom.Application.Course.Contracts;
using FluentValidation;

namespace DPBloom.Application.Course.Validators;

public class UpdateCourseValidator : AbstractValidator<UpdateCourse>
{
    public UpdateCourseValidator()
    {
        RuleFor(uc => uc).NotNull().WithMessage("Course object cannot be null.");
        
        RuleFor(uc => uc.Title)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(uc => !string.IsNullOrEmpty(uc.Title))
            .WithMessage("Title must be at least 3 characters long.");
        
        //TODO: check, if it working properly
        RuleFor(uc => uc.Description)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleFor(uc => uc.AuthorId)
            .NotEmpty().WithMessage("AuthorId is required.");
    }
}