using DPBloom.Application.Exam.Contracts;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class SubmitAnswerValidator : AbstractValidator<SubmitAnswerDto>
{
    public SubmitAnswerValidator()
    {
        RuleFor(sa => sa)
            .NotNull().WithMessage("Answer object cannot be null.");
        
        RuleFor(sa => sa.QuestionId)
            .NotEmpty().WithMessage("Question id is required.");
        
        RuleFor(sa => sa.SelectedOptionIds)
            .NotEmpty().When(x => x.SelectedOptionIds is not null)
            .WithMessage("Selected option ids are required.");

        RuleFor(sa => sa.FreeTextAnswer)
            .NotEmpty().When(x => x.FreeTextAnswer is not null)
            .WithMessage("Free text answer is required.")
            .MaximumLength(1111).WithMessage("Title must be at most 1111 characters long.");
    }
}