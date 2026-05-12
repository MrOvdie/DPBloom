using DPBloom.Application.Exam;
using DPBloom.Application.Lecture;
using DPBloom.Application.Topic;

namespace DPBloom.Application.Course.Contracts;

public class CourseAggregateDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public bool IsFinished { get; set; }

    public IReadOnlyList<TopicDto>? Topics { get; set; }
    public IReadOnlyList<LectureDto>? Lectures { get; set; }
    public IReadOnlyList<ExamDetailsDto>? Exams { get; set; }
}