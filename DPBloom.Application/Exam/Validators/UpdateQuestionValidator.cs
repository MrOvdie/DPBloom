using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Update;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionDto>
{
    public UpdateQuestionValidator()
    {
        RuleFor(q => q.Text)
            .NotEmpty().When(x => x.Text is not null)
            .WithMessage("Question text is required.")
            .MaximumLength(500).WithMessage("Question text cannot be longer than 500 characters.");

        RuleFor(q => q.Type)
            .InclusiveBetween(0, 5).WithMessage("Invalid question type."); //TODO: make proper ranges

        RuleFor(q => q.Level)
            .InclusiveBetween(0, 6).WithMessage("Invalid Bloom's taxonomy level.");

        RuleFor(q => q.CheckingType)
            .InclusiveBetween(0, 2).WithMessage("Invalid checking type.");

        RuleForEach(q => q.Options)
            .SetValidator(new UpdateOptionValidator());
    }
}