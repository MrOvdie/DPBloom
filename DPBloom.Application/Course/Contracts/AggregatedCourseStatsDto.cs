using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.Course.Contracts;

public class AggregatedCourseStatsDto
{
    public Guid CourseId { get; set; }
    public double Score { get; set; }
    public double CourseCompletion { get; set; }
    public double ExamCompletion { get; set; }
}