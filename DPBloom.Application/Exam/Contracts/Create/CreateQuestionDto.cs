namespace DPBloom.Application.Exam.Contracts.Create;

public class CreateQuestionDto
{
    public string Text { get; set; }
    public int Type { get; set; }
    public double ScoreWeight { get; set; }
    public int CheckingType { get; set; }
    public int Level { get; set; }
    
    public List<CreateOptionDto> Options { get; set; }
}