using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class SubmitAnswerDto : ModelBase<Guid>
{
    // Для якого питання надсилаємо відповідь
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }

    // Для SingleChoice/MultipleChoice    
    public List<Guid>? SelectedOptionIds { get; set; }

    // Для OpenAnswer  
    public string? FreeTextAnswer { get; set; }
}