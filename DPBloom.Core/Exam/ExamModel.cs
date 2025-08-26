namespace DPBloom.Core.Exam;

public class ExamModel : EntityBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string CourseId { get; set; }
    public string AuthorId { get; set; }
    public string? TopicId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime FinishesAt { get; set; }
    public bool CanSkip { get; set; }
    public bool ShowResults { get; set; }
    public bool IsRandomOrder { get; set; }
}