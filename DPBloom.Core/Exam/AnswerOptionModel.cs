using DPBloom.Core.Base;

namespace DPBloom.Core.Exam;

public class AnswerOptionModel : EntityBase<Guid>
{
    public string Text { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
}