namespace DPBloom.Application.Exam.Contracts;

public class AttemptResultDto
{
    public Guid AttemptId { get; set; }
    public Guid ExamId { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public double Score { get; set; }
    public double ScorePercentage { get; set; }
    
    public bool Passed { get; set; }

    public List<QuestionResultDto> Details { get; set; }
}