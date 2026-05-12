using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Extensions;
using DPBloom.Infrastructure.User;

namespace DPBloom.Infrastructure.Exam;

public class UserExamAttemptDao : EntityDaoBase<Guid>, ISoftDelete
{
    public Guid ExamId { get; set; }
    public Guid? AttemptResultId { get; set; }
    public Guid UserId { get; set; }
    public int Status { get; set; }
    public double? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public bool IsValid { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public ExamDao Exam { get; set; }
    public ICollection<UserAnswerDao> Answers { get; set; }
    public ApplicationUser User { get; set; }
    public AttemptResultDao? AttemptResult { get; set; }
}