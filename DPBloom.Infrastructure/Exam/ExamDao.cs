using DPBloom.Infrastructure.Base;

namespace DPBloom.Infrastructure.Exam;

public class ExamDao : EntityDaoBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? TopicId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime FinishesAt { get; set; }
    public bool CanSkip { get; set; }
    public bool ShowResults { get; set; }
    public bool IsRandomOrder { get; set; }

    public ICollection<QuestionDao> Questions { get; set; }
}