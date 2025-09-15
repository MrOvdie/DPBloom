using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class UpdateQuestionDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public int Type { get; set; }
    public int CheckingType { get; set; }
    public int Category { get; set; }
    
    public List<UpdateOptionDto>? Options { get; set; }
}