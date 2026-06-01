using DPBloom.Application.Exam.Contracts;
using DPBloom.Application.Lecture.Contracts;
using DPBloom.Application.Topic;

namespace DPBloom.Application.Course.Contracts;

public class CourseAggregateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; }
    public bool IsFinished { get; set; }

    public IReadOnlyList<TopicDto>? Topics { get; set; }
    public IReadOnlyList<LectureDto>? Lectures { get; set; }
    public IReadOnlyList<ExamRecordDto>? Exams { get; set; }
}