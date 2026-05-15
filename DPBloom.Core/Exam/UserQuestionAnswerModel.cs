using DPBloom.Core.Base;

namespace DPBloom.Core.Exam;

public class UserQuestionAnswerModel : EntityBase<Guid>
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public IEnumerable<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }
    public DateTime SubmittedAt { get; set; }
}