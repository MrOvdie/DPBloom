using DPBloom.Application.Lecture.Contracts;
using FluentValidation;

namespace DPBloom.Application.Lecture.Validators;

public class UpdateLectureValidator : AbstractValidator<UpdateLecture>
{
    public UpdateLectureValidator()
    {
        RuleFor(ul => ul.Title)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Title))
            .WithMessage("Title must be at least 3 characters long.");
        
        RuleFor(ul => ul.Description)
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Description))
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleForEach(ul => ul.ContentLinks)
            .Must(link => string.IsNullOrEmpty(link) || Uri.IsWellFormedUriString(link, UriKind.Absolute))
            .WithMessage("ContentLink must be a valid URL");
        
        //TODO: check if it working properly
        RuleForEach(ul => ul.FilePaths)
            .MaximumLength(260)
            .WithMessage("FilePath must be at most 260 characters long.")
            .Must(path => string.IsNullOrEmpty(path) || Path.IsPathFullyQualified(path))
            .WithMessage("FilePath must be a valid absolute path.");

    }
}