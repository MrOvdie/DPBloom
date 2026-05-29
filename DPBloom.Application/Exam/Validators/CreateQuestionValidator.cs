using DPBloom.Application.Exam.Contracts.Create;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class CreateQuestionValidator : AbstractValidator<CreateQuestionDto>
{
    public CreateQuestionValidator()
    {
        RuleFor(q => q.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(500).WithMessage("Question text cannot be longer than 500 characters.");

        RuleFor(q => q.Type)
            .IsInEnum().WithMessage("Invalid question type.");

        RuleFor(q => q.Level)
            .IsInEnum().WithMessage("Invalid Bloom's taxonomy level.");

        RuleFor(q => q.CheckingType)
            .IsInEnum().WithMessage("Invalid checking type.");

        RuleFor(q => q.Options)
            .NotEmpty().WithMessage("Question must have options.")
            .Must(options => options == null || options.Count >= 2)
            .WithMessage("At least 2 options are required for a choice-based question.");
        
        RuleForEach(q => q.Options)
            .SetValidator(new CreateOptionValidator());
    }
}