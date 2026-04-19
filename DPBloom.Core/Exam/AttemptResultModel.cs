using DPBloom.Core.Base;

namespace DPBloom.Core.Exam;

public class AttemptResultModel : EntityBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid ExamId { get; set; }
    public Guid UserId { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public double Score { get; set; }
    public double ScorePercentage { get; set; }
    
    public bool Passed { get; set; }
    
    public List<QuestionResultModel> Details { get; set; }
}