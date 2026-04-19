using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom.Contracts;

public  class BloomLevelGroupPerformanceDto
{
    public BloomLevel Level { get; set; }
    public double AverageScorePercentage { get; set; }
    public double PassRatePercentage { get; set; }
    public bool IsGroupWeakPoint { get; set; }
}