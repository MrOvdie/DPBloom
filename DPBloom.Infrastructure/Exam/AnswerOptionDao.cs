using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public class AnswerOptionDao : EntityDaoBase<Guid>
{
    public string Text { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    
    public QuestionDao Question { get; set; }
}