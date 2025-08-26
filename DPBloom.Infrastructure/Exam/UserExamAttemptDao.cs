using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public class UserExamAttemptDao : EntityDaoBase<Guid>
{
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }
    public int Status { get; set; }
    public double? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public bool IsValid { get; set; }
    
    public ExamDao Exam { get; set; }
    public ICollection<UserAnswerDao> Answers { get; set; }
}