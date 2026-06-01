using DPBloom.Application.Exam.Contracts.Create;
using DPBloom.Core.Exam.Enums;
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
            .Must(options => options is not { Count: < 2 })
            .Unless(q => q.Type == QuestionType.OpenAnswer)
            .WithMessage("At least 2 options are required for a choice-based question.");
        
        RuleForEach(q => q.Options)
            .SetValidator(new CreateOptionValidator())
            .When(q => q.CheckingType == CheckingType.Automatic);
    }
}