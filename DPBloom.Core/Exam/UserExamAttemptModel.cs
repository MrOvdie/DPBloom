using DPBloom.Core.Base;
using DPBloom.Core.Exam.Enums;
using TestOfTesting.Models.Enums;

namespace DPBloom.Core.Exam;

public class UserExamAttemptModel : EntityBase<Guid>
{
    public Guid ExamId { get; set; }
    public Guid? AttemptResultId { get; set; }
    public Guid UserId { get; set; }
    public AttemptStatus Status { get; set; }
    public double? TotalScore { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public bool IsValid { get; set; }
}