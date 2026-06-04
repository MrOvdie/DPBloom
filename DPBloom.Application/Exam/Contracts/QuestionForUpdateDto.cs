using DPBloom.Application.Base;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Exam.Contracts;

public class QuestionForUpdateDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public QuestionType Type { get; set; }          
    public BloomLevel Level { get; set; }     
    public CheckingType CheckingType { get; set; }  
    public double ScoreWeight { get; set; }  
    public List<OptionForUpdateDto> Options { get; set; }
}