using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts.Update;

public class UpdateQuestionDto : ModelBase<Guid>
{
    public string? Text { get; set; }
    public int? Type { get; set; }
    public double? ScoreWeight { get; set; }
    
    public int? CheckingType { get; set; }
    public int? Level { get; set; }
    
    public List<UpdateOptionDto>? Options { get; set; }
}