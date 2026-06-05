using DPBloom.Application.Lecture.Contracts;
using FluentValidation;

namespace DPBloom.Application.Lecture.Validators;

public class UpdateLectureValidator : AbstractValidator<UpdateLectureDto>
{
    public UpdateLectureValidator()
    {
        RuleFor(ul => ul).NotNull().WithMessage("Lecture object cannot be null.");
        
        RuleFor(ul => ul.Title)
            .NotEmpty().When(x => x.Title is not null)
            .MaximumLength(150).WithMessage("Title must be at most 150 characters long.")
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Title))
            .WithMessage("Title must be at least 3 characters long.");
        
        RuleFor(ul => ul.Description)
            .NotEmpty().When(x => x.Description is not null)
            .MinimumLength(3).When(ul => !string.IsNullOrEmpty(ul.Description))
            .WithMessage("Description must be at least 3 characters long.");
        
        RuleFor(cl => cl.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(30000).WithMessage("Content must be at most 30000 characters long.");
        
        RuleForEach(ul => ul.ContentLinks)
            .Must(link => string.IsNullOrEmpty(link) || Uri.IsWellFormedUriString(link, UriKind.Absolute))
            .WithMessage("ContentLink must be a valid URL");
        
        RuleFor(ul => ul.CourseId)
            .NotEmpty().When(x => x.CourseId != null)
            .WithMessage("CourseId can't be empty.");
        
        RuleFor(ul => ul.TopicId)
            .NotEmpty().When(x => x.TopicId != null)
            .WithMessage("TopicId can't be empty.");
        
        RuleForEach(ul => ul.FilePaths)
            .MaximumLength(360)
            .WithMessage("FilePath must be at most 360 characters long.")
            .Must(path => string.IsNullOrEmpty(path) || Path.IsPathFullyQualified(path))
            .WithMessage("FilePath must be a valid absolute path.");
    }
}