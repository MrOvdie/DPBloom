using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Extensions;
using DPBloom.Infrastructure.User;

namespace DPBloom.Infrastructure.Exam;

public class AttemptResultDao : EntityDaoBase<Guid>, ISoftDelete
{
    public Guid AttemptId { get; set; }
    public Guid ExamId { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }

    public double Score { get; set; }
    public double MaxScore { get; set; }
    public double ScorePercentage { get; set; }
    public TimeSpan? Duration { get; set; }
    
    public DateTime EvaluatedOn { get; set; }

    public bool Passed { get; set; }

    public List<QuestionResultDao> Details { get; set; }
    public UserExamAttemptDao Attempt { get; set; }
    public ExamDao Exam { get; set; }
    public CourseDao Course { get; set; }
    public ApplicationUser User { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}