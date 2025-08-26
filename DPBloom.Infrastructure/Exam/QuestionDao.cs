using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public class QuestionDao : EntityDaoBase<Guid>
{
    public string Text { get; set; }
    public Guid ExamId { get; set; }
    public int Position { get; set; }
    public double ScoreWeight { get; set; }
    public int Type { get; set; }
    public int Category { get; set; }
    public int CheckingType { get; set; }
    
    public ExamDao Exam { get; set; }
    public ICollection<AnswerOptionDao> Options { get; set; }
}