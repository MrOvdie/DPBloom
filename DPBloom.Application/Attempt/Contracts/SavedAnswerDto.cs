namespace DPBloom.Application.Attempt.Contracts;

public class SavedAnswerDto
{
    public Guid QuestionId { get; set; }
    
    public List<Guid> SelectedOptionIds { get; set; } = new();
    
    public string? FreeTextAnswer { get; set; } 
}