using DPBloom.Application.Lecture.Contracts;
using FluentValidation;

namespace DPBloom.Application.Lecture.Validators;

public class CreateLectureValidator : AbstractValidator<CreateLecture>
{
    public CreateLectureValidator()
    {
        RuleFor(cl => cl).NotNull().WithMessage("Lecture object cannot be null.");
        
        RuleFor(cl => cl.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long.")
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.");
        
        RuleFor(cl => cl.Description)
            .Must(d => string.IsNullOrEmpty(d) || d.Length >= 3)
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleFor(cl => cl.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(30000).WithMessage("Content must be at most 30000 characters long.");

        RuleForEach(cl => cl.ContentLinks)
            .Must(link => string.IsNullOrEmpty(link) || Uri.IsWellFormedUriString(link, UriKind.Absolute))
            .WithMessage("ContentLink must be a valid URL");
        
        RuleForEach(cl => cl.FilePaths)
            .MaximumLength(260)
            .WithMessage("FilePath must be at most 260 characters long.")
            .Must(path => string.IsNullOrEmpty(path) || Path.IsPathFullyQualified(path))
            .WithMessage("FilePath must be a valid absolute path.");
    }
}