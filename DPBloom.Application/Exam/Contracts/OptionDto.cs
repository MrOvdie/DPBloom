using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class OptionDto : ModelBase<Guid>
{
    public string Text { get; set; }
}