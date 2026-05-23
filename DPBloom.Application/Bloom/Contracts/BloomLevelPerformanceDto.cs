using DPBloom.Core.Exam.Enums;

namespace DPBloom.Application.Bloom.Contracts;

public class BloomLevelPerformanceDto
{
    public BloomLevel Level { get; set; } // "Knowing", "Applying"
    public double ScorePercentage { get; set; }
    public bool IsWeakPoint { get; set; }
}