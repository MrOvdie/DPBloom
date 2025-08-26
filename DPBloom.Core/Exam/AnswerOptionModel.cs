namespace DPBloom.Core.Exam;

public class AnswerOptionModel : EntityBase<Guid>
{
    public string Text { get; set; }
    public string QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    
    //public QuestionModel Question { get; set; }
}