using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class OptionForUpdateDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}