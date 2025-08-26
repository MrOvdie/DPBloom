namespace DPBloom.Core.Exam;

public class UserAnswerModel : EntityBase<Guid>
{
    public string AttemptId { get; set; }
    public string QuestionId { get; set; }
    public ICollection<string>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }
}