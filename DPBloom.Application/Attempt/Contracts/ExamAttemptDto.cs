using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Attempt.Contracts;

public class ExamAttemptDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid CourseId { get; set; }
    public Guid? AttemptResultId { get; set; }
    public Guid UserId { get; set; }
    public AttemptStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}