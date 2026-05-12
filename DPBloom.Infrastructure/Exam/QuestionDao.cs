using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Extensions;

namespace DPBloom.Infrastructure.Exam;

public class QuestionDao : EntityDaoBase<Guid>, ISoftDelete
{
    public string Text { get; set; }
    public Guid ExamId { get; set; }
    public int Position { get; set; }
    public double ScoreWeight { get; set; }
    public int Type { get; set; }
    public int Level { get; set; }
    public int CheckingType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public ExamDao Exam { get; set; }
    public ICollection<AnswerOptionDao> Options { get; set; }
}