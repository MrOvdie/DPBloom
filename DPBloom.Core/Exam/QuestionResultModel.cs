using DPBloom.Core.Base;
using DPBloom.Core.Exam.Enums;

namespace DPBloom.Core.Exam;

public class QuestionResultModel : EntityBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid AttemptResultId { get; set; }
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
    public double Score { get; set; }
    public double MaxScore { get; set; }
    public bool IsCorrect { get; set; }
    public string? Comment { get; set; }

    public AttemptStatus QuestionResultStatus { get; set; }

    public IEnumerable<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }

    public IEnumerable<Guid>? CorrectOptionIds { get; set; }
    public string? CorrectAnswerText { get; set; }
}