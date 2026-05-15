namespace DPBloom.Application.Exam.Contracts;

public class ExamRecordDto
{
    public Guid ExamId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid CourseId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? TopicId { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime FinishesAt { get; set; }
}