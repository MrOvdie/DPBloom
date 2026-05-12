using DPBloom.Application.Exam.Contracts.Create;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class CreateOptionValidator : AbstractValidator<CreateOptionDto>
{
    public CreateOptionValidator()
    {
        RuleFor(o => o.Text)
            .NotEmpty().WithMessage("Option text is required.");
        // .MaximumLength(200).WithMessage("Option text cannot be longer than 200 characters.");
    }
}