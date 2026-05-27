using DPBloom.Application.Bloom.Contracts;

namespace DPBloom.Application.User.Contracts;

public class GlobalUserDashboardDto
{
    public double AverageCourseScore { get; set; }
    public double AverageExamCompletion { get; set; }
    public List<BloomLevelPerformanceDto> BloomPerformance { get; set; } = new();
}