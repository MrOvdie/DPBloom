namespace DPBloom.Application.Attempt.Contracts;

public class AttemptResultRecordDto
{
    public Guid Id { get; set; }
    public Guid ResultId { get; set; }
    
    public Guid AttemptId { get; set; }
    public Guid ExamId { get; set; }
    public Guid CourseId { get; set; }

    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }

    public double? Score { get; set; }
    public double MaxScore { get; set; }
    public double? ScorePercentage { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public bool? Passed { get; set; }
}