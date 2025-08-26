using TestOfTesting.Models.Enums;

namespace DPBloom.Core.Exam;

public class UserExamAttemptModel : EntityBase<Guid>
{
    public string ExamId { get; set; }
    public string UserId { get; set; }
    public AttemptStatus Status { get; set; }
    public double? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public bool IsValid { get; set; }
    
    // public Exam Exam { get; set; }
    // public ICollection<UserAnswer> Answers { get; set; }
}