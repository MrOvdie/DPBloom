using DPBloom.Application.Base;

namespace DPBloom.Application.Exam.Contracts;

public class ExamRecordDto : ModelBase<Guid>
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid LastUpdaterId { get; set; }
    public Guid? TopicId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime FinishesAt { get; set; }
    public double MinimalPassScore { get; set; }
    public int AttemptsCount { get; set; }
    public bool CanCheckAttempts { get; set; }
}