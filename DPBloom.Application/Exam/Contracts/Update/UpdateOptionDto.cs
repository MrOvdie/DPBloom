using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts.Update;

public class UpdateOptionDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}