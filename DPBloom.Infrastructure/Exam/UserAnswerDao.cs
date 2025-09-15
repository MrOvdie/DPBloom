using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public class UserAnswerDao : EntityDaoBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public ICollection<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }
    public DateTime SubmittedAt { get; set; }
    
    public UserExamAttemptDao Attempt { get; set; }
    public QuestionDao Question { get; set; }
}