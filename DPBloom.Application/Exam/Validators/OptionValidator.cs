using FluentValidation;
using TestOfTesting.DTOs;

namespace DPBloom.Application.Exam.Validators;

public class OptionValidator : AbstractValidator<CreateOptionDto>
{
    public OptionValidator()
    {
        RuleFor(o => o.Text)
            .NotEmpty().WithMessage("Option text is required.");
        // .MaximumLength(200).WithMessage("Option text cannot be longer than 200 characters.");
    }
}