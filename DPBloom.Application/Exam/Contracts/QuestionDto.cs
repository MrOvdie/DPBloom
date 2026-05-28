using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class QuestionDto : ModelBase<Guid>
{
    public string Text { get; set; }
    public int Type { get; set; }          
    public int Level { get; set; }     
    public int CheckingType { get; set; }  
    public List<OptionDto> Options { get; set; }
}