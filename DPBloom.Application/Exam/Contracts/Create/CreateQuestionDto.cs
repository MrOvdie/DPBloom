using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Exam.Contracts.Create;

public class CreateQuestionDto
{
    public string Text { get; set; }
    public QuestionType Type { get; set; }
    public double ScoreWeight { get; set; }
    public CheckingType CheckingType { get; set; }
    public BloomLevel Level { get; set; }
    
    public List<CreateOptionDto> Options { get; set; }
}