using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class SubmitAnswerDto : ModelBase<Guid>
{
    public Guid QuestionId { get; set; }
    
    public List<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }
}