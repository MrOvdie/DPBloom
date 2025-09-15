using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Data;

namespace DPBloom.Infrastructure.Exam;

public class AttemptResultDao : EntityDaoBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid ExamId { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public double Score { get; set; }
    public double ScorePercentage { get; set; }
    
    public bool Passed { get; set; }

    public List<QuestionResultDao> Details { get; set; }
    public UserExamAttemptDao Attempt { get; set; }
    public ExamDao Exam { get; set; }
}