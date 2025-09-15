using DPBloom.Application.Base;

namespace DPBloom.Application.Exam;

public class QuestionDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public int Type { get; set; }          
    public int Category { get; set; }     
    public int CheckingType { get; set; }  
    public List<OptionDto> Options { get; set; }
}