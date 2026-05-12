using DPBloom.Infrastructure.Base;
using DPBloom.Infrastructure.Exam;

namespace DPBloom.Infrastructure.Data;

public class QuestionResultDao : EntityDaoBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid AttemptResultId { get; set; }
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
    public double Score { get; set; }
    public bool IsCorrect { get; set; }

    public IEnumerable<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }

    public IEnumerable<Guid>? CorrectOptionIds { get; set; }
    public string? CorrectAnswerText { get; set; }

    public QuestionDao Question { get; set; }
    public UserExamAttemptDao Attempt { get; set; }
    public AttemptResultDao AttemptResult { get; set; }
}