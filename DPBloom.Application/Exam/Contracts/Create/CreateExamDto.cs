using DPBloom.Application.Base;

namespace TestOfTesting.DTOs;

public class CreateExamDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime FinishesAt { get; set; }
    public bool CanSkip { get; set; }
    public bool ShowResults { get; set; }
    public bool IsRandomOrder { get; set; }

    public List<CreateQuestionDto> Questions { get; set; }
}
