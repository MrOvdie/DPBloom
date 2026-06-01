using DPBloom.Application.Exam.Contracts;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Attempt.Contracts;

public class AttemptDetailsDto
{
    public Guid Id { get; set; }
    public Guid? AttemptResultId { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; }
    public string? ExamDescription { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime StartedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public AttemptStatus Status { get; set; }
    
    public List<QuestionDto> Questions { get; set; } = new();
    
    public List<SavedAnswerDto> SavedAnswers { get; set; } = new();
}