using DPBloom.Application.Course.Contracts;
using FluentValidation;

namespace DPBloom.Application.Course.Validators;

public class UpdateCourseValidator : AbstractValidator<UpdateCourse>
{
    public UpdateCourseValidator()
    {
        RuleFor(uc => uc).NotNull().WithMessage("Course object cannot be null.");
        
        RuleFor(uc => uc.Title)
            .NotEmpty().When(x => x.Title is not null)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(uc => !string.IsNullOrEmpty(uc.Title))
            .WithMessage("Title must be at least 3 characters long.");
        
        //TODO: check, if it is working properly
        RuleFor(uc => uc.Description)
            .NotEmpty().When(x => x.Description is not null)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
    }
}