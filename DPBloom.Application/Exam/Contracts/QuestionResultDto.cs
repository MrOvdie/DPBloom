using DPBloom.Application.Base;

namespace DPBloom.Application.Exam;

public class QuestionResultDto
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
    public double Score { get; set; }
    public bool IsCorrect { get; set; }

    // Що обрав користувач
    public IEnumerable<Guid>? SelectedOptionIds { get; set; }
    public string? FreeTextAnswer { get; set; }

    // Правильні опції (для UI після завершення)
    public IEnumerable<Guid>? CorrectOptionIds { get; set; }
    public string? CorrectAnswerText { get; set; }
}