using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Exam.Contracts.Update;
using FluentValidation;

namespace DPBloom.Application.Exam.Validators;

public class UpdateOptionValidator : AbstractValidator<UpdateOptionDto>
{
    public UpdateOptionValidator()
    {
        RuleFor(o => o.Text)
            .NotEmpty().When(x => x.Text is not null)
            .WithMessage("Option text is required.");
        //.MaximumLength(200).WithMessage("Option text cannot be longer than 200 characters.");
    }
}