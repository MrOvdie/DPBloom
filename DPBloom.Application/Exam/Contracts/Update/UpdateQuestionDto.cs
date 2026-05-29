using DPBloom.Application.Base;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Exam.Contracts.Update;

public class UpdateQuestionDto
{
    public Guid? Id { get; set; }
    public string? Text { get; set; }
    public QuestionType? Type { get; set; }
    public double? ScoreWeight { get; set; }
    
    public CheckingType? CheckingType { get; set; }
    public BloomLevel? Level { get; set; }
    
    public List<UpdateOptionDto>? Options { get; set; }
}